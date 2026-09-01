
#include "sequence.h"
#include "connection.h"
#include "protocol.h"
#include "allowed.h"
#include "crc.h"
#include <ctype.h>
#include <string.h>

void check_fragment_finished(Connection* con);
int process_first_fragment(Connection* con, uint8_t* data, int len);
void filter_server_list(Connection* con, int totalLen);

uint16_t get_sequence(uint8_t* data)
{
    return ToHostShort(*(uint16_t*)(&data[2]));
}

void sequence_init(Connection* con)
{
    Sequence* seq = &con->sequence;
    memset(seq, 0, sizeof(Sequence));
}

void sequence_free(Connection* con)
{
    Sequence* seq = &con->sequence;
    uint16_t i;
    uint16_t count;

    if (!seq->packets)
        return;

    count = seq->count;
    for (i = 0; i < count; i++)
    {
        if (seq->packets[i].data)
            free(seq->packets[i].data);
    }

    free(seq->packets);
    sequence_init(con);
}

void grow(Connection* con, uint32_t index)
{
    Sequence* seq = &con->sequence;
    uint32_t cap = 32;
    Packet* array;

    while (cap <= index)
        cap *= 2;

    array = (Packet*)calloc(cap, sizeof(Packet));
    if (!array)
        longjmp(con->jmpBuf, ERR_BAD_ALLOC);

    if (seq->capacity != 0)
    {
        memcpy(array, seq->packets, seq->capacity * sizeof(Packet));
        free(seq->packets);
    }

    seq->capacity = cap;
    seq->packets = array;
}

void sequence_adjust_ack(Connection* con, uint8_t* data, int len)
{
    if (len < 4)
        return;

    *(uint16_t*)(&data[2]) = ToNetworkShort(con->sequence.seqFromRemote - 1);
}

Packet* get_packet_space(Connection* con, uint16_t sequence, int len)
{
    Sequence* seq = &con->sequence;
    Packet* p;

    if (sequence >= seq->count)
        seq->count = sequence + 1;

    if (sequence >= seq->capacity)
        grow(con, sequence + 1);

    p = &seq->packets[sequence];
    if (p->data)
        free(p->data);

    p->len = len;
    p->data = NULL;

    return p;
}

void copy_fragment(Connection* con, Packet* p, uint8_t* data, int len)
{
    uint8_t* copy = (uint8_t*)malloc(len);
    if (!copy)
        longjmp(con->jmpBuf, ERR_BAD_ALLOC);
    memcpy(copy, data, len);

    p->data = copy;
}

void sequence_recv_packet(Connection* con, uint8_t* data, int len)
{
    Sequence* seq = &con->sequence;
    uint16_t val = get_sequence(data);
    Packet* p;
    int prevAssigned;
    uint16_t prevOut;
    uint32_t i;

    /* A retransmit (the client's ack was lost) must keep the sequence it was
     * relabeled to the first time, and must not advance seqToLocal again. */
    prevAssigned = 0;
    prevOut = 0;
    if (val < seq->count && seq->packets[val].outAssigned)
    {
        prevAssigned = 1;
        prevOut = seq->packets[val].outSeq;
    }

    p = get_packet_space(con, val, len);
    p->isFragment = 0;

    /* Correct the sequence for the client (we may have swallowed fragments) */
    if (prevAssigned)
    {
        p->outSeq = prevOut;
        p->outAssigned = 1;
        *(uint16_t*)(&data[2]) = ToNetworkShort(prevOut);
    }
    else
    {
        p->outSeq = seq->seqToLocal;
        p->outAssigned = 1;
        *(uint16_t*)(&data[2]) = ToNetworkShort(seq->seqToLocal++);
    }

    if (val != seq->seqFromRemote)
        return;

    for (i = val; i < seq->count; i++)
    {
        if (seq->packets[i].len > 0)
        {
            seq->seqFromRemote++;

            if (seq->packets[i].isFragment && process_first_fragment(con, seq->packets[i].data, seq->packets[i].len))
            {
                check_fragment_finished(con);
                break;
            }
        }
    }
}

void sequence_recv_fragment(Connection* con, uint8_t* data, int len)
{
    Sequence* seq = &con->sequence;
    uint16_t val = get_sequence(data);
    Packet* p = get_packet_space(con, val, len);

    p->isFragment = 1;
    copy_fragment(con, p, data, len);

    if (val == seq->seqFromRemote)
        process_first_fragment(con, data, len);
    else if (seq->fragTotal > 0)
        check_fragment_finished(con);
}

void sequence_adjust_combined(Connection* con, int len)
{
    int sublen;
    int pos = 2;
    uint8_t* data;

    if (len < 4)
        return;

    for (;;)
    {
        sublen = con->buffer[pos];
        pos++;
        if ((pos + sublen) > len || sublen == 0)
            return;

        data = &con->buffer[pos];
        if (ToHostShort(*(uint16_t*)data) == 0x15)
            sequence_adjust_ack(con, data, sublen);

        pos += sublen;
        if (pos >= len)
            return;
    }
}

void sequence_recv_combined(Connection* con, uint8_t* data, int len)
{
    int sublen;
    int pos = 2;

    if (len < 4)
        return;

    for (;;)
    {
        sublen = data[pos];
        pos++;
        if ((pos + sublen) > len || sublen == 0)
            return;

        recv_from_remote(con, &data[pos], sublen);

        pos += sublen;
        if (pos >= len)
            return;
    }
}

int process_first_fragment(Connection* con, uint8_t* data, int len)
{
    Sequence* seq = &con->sequence;
    uint16_t appOpcode;

    if (len < (int)sizeof(FirstFrag))
        return 0;

    /* AppOpcode is little-endian on the wire (the original C compared it
     * without a byte swap; the Go port reads LE). */
    appOpcode = (uint16_t)(data[8] | (data[9] << 8));

    if (appOpcode != 0x18) /* OP_ServerListResponse */
        return 0;

    seq->fragStart = get_sequence(data);
    /* TotalLength is big-endian: the expected server-list payload bytes.
     * Track a byte count (not a predicted fragment count) so reassembly
     * works regardless of datagram sizes / CRCs. */
    seq->fragTotal = ((uint32_t)data[4] << 24) | ((uint32_t)data[5] << 16)
                   | ((uint32_t)data[6] << 8) | data[7];
    return 1;
}

void check_fragment_finished(Connection* con)
{
    Sequence* seq = &con->sequence;
    uint32_t index = seq->fragStart;
    int got;
    Packet* p;

    if (seq->fragTotal == 0)
        return;

    if (index >= seq->count)
        return;

    p = &seq->packets[index];
    if (p->len == 0 || !p->data)
        return;

    got = p->len - sizeof(FirstFrag) + 2; /* AppOpcode is counted */

    while (got < (int)seq->fragTotal)
    {
        index++;
        if (index >= seq->count)
            return;

        p = &seq->packets[index];
        if (!p->data)
            return;

        got += p->len - sizeof(Frag);
    }

    /* If we reach here, we had the whole sequence! */
    filter_server_list(con, (int)seq->fragTotal - 2); /* Don't count AppOpcode here */
}

int compare_prefix(const char* a, const char* b, int len)
{
    int i;
    for (i = 0; i < len; i++)
    {
        if (!a[i] || !b[i])
            return 0;
        if (tolower((unsigned char)a[i]) != tolower((unsigned char)b[i]))
            return 0;
    }

    return 1;
}

void filter_server_list(Connection* con, int totalLen)
{
    Sequence* seq = &con->sequence;
    uint32_t index = seq->fragStart;
    int pos, i, outCount;
    Packet* p;
    uint8_t* serverList;
    uint8_t* outBuffer;
    int outBufSize = totalLen + 26 + 4; /* header + room for all servers + CRC headroom */
    int outLen = 0;
    char* name;

    if (index >= seq->count)
        return;

    p = &seq->packets[index];
    if (p->len == 0)
        return;

    serverList = (uint8_t*)malloc(totalLen);
    if (!serverList)
        longjmp(con->jmpBuf, ERR_BAD_ALLOC);

    outBuffer = (uint8_t*)malloc(outBufSize);
    if (!outBuffer)
    {
        free(serverList);
        longjmp(con->jmpBuf, ERR_BAD_ALLOC);
    }

    memcpy(serverList, p->data + sizeof(FirstFrag), p->len - sizeof(FirstFrag));
    pos = p->len - sizeof(FirstFrag); /* Not counting AppOpcode */

    while (pos < totalLen)
    {
        index++;
        if (index >= seq->count)
        {
            free(serverList);
            free(outBuffer);
            return;
        }
        p = &seq->packets[index];

        memcpy(serverList + pos, p->data + sizeof(Frag), p->len - sizeof(Frag));
        pos += p->len - sizeof(Frag);
    }

    /* We now have the whole server list in one piece */
    /* Write our output packet header */
    outBuffer[0] = 0;
    outBuffer[1] = 0x09; /* OP_Packet */
    *(uint16_t*)(&outBuffer[2]) = ToNetworkShort(seq->seqToLocal++);
    outBuffer[4] = 0x18; /* OP_ServerListResponse */
    outBuffer[5] = 0;

    /* First 16 bytes of the server list packet is some kind of header, copy it over */
    for (i = 0; i < 16; i++)
        outBuffer[i + 6] = serverList[i];

    /* outBuffer[22] is a 4-byte count of the number of servers on the list -- we don't know the value yet, probably 2 */
    outLen = 16 + 6 + 4;
    outCount = 0;

    /* List of servers starts at serverList[20] */
    pos = 20;

    while (pos < totalLen)
    {
        /* Server listings are variable-size */
        i = pos; /* Start of this server in serverList */
        pos += strlen((char*)(serverList + pos)) + 1; /* IP address */
        pos += sizeof(int) * 2; /* ListId, runtimeId */
        name = (char*)(serverList + pos);
        pos += strlen(name) + 1;
        pos += strlen((char*)(serverList + pos)) + 1; /* language */
        pos += strlen((char*)(serverList + pos)) + 1; /* region */
        pos += sizeof(int) * 2; /* Status, player count */

        /* Time to check the name! */
        if (compare_prefix(name, SERVER_NAME_PREFIX, sizeof(SERVER_NAME_PREFIX) - 1)
            || allowed_match(&g_allowed, name))
        {
            outCount++;
            memcpy(outBuffer + outLen, serverList + i, pos - i);
            outLen += pos - i;
        }
    }

    /* Write our outgoing server count */
    *(int*)(&outBuffer[22]) = outCount;

    seq->seqFromRemote = index + 1;
    seq->fragTotal = 0;
    seq->fragStart = 0;
    free(serverList);

    if (outLen <= BUFFER_SIZE)
    {
        memcpy(con->buffer, outBuffer, outLen);
        free(outBuffer);
        connection_send(con, con->buffer, outLen, 0);
    }
    else
    {
        /* connection_send only appends a CRC when it fits txBuf; for an
         * oversized rebuilt server list we append it here ourselves. */
        if (con->crcBytes != 0 && !packet_crc_exempt(outBuffer))
        {
            uint32_t crc = packet_crc(outBuffer, outLen, con->crcKey);
            if (con->crcBytes == 2)
            {
                outBuffer[outLen] = (uint8_t)(crc >> 8);
                outBuffer[outLen + 1] = (uint8_t)(crc & 0xff);
                outLen += 2;
            }
            else if (con->crcBytes == 4)
            {
                outBuffer[outLen] = (uint8_t)(crc >> 24);
                outBuffer[outLen + 1] = (uint8_t)(crc >> 16);
                outBuffer[outLen + 2] = (uint8_t)(crc >> 8);
                outBuffer[outLen + 3] = (uint8_t)crc;
                outLen += 4;
            }
        }
        connection_send(con, outBuffer, outLen, 0);
        free(outBuffer);
    }
}
