#include "crc.h"

static uint32_t crc_table[256];
static int crc_table_ready;

static void crc_table_init(void)
{
    uint32_t c;
    int n, k;

    if (crc_table_ready)
        return;

    for (n = 0; n < 256; n++)
    {
        c = (uint32_t)n;
        for (k = 0; k < 8; k++)
            c = (c & 1) ? (0xEDB88320u ^ (c >> 1)) : (c >> 1);
        crc_table[n] = c;
    }

    crc_table_ready = 1;
}

static uint32_t crc_step(uint32_t crc, const uint8_t* data, int len)
{
    int i;

    for (i = 0; i < len; i++)
        crc = (crc >> 8) ^ crc_table[(crc ^ data[i]) & 0xff];

    return crc;
}

int packet_crc_exempt(const uint8_t* data)
{
    uint16_t opcode = (uint16_t)((data[0] << 8) | data[1]);

    return opcode == 0x01 || opcode == 0x02 || opcode == 0x11;
}

uint32_t packet_crc(const uint8_t* data, int len, uint32_t key)
{
    uint8_t keyBytes[4];
    uint32_t crc;

    crc_table_init();

    /* Feed the session key first, least-significant byte first (the byte
     * order EQEmu's EQ::Crc32 and the Go port consume it in). */
    keyBytes[0] = (uint8_t)(key & 0xff);
    keyBytes[1] = (uint8_t)((key >> 8) & 0xff);
    keyBytes[2] = (uint8_t)((key >> 16) & 0xff);
    keyBytes[3] = (uint8_t)((key >> 24) & 0xff);

    crc = crc_step(0xffffffffu, keyBytes, 4);
    crc = crc_step(crc, data, len);

    return ~crc;
}

static uint32_t byteswap32(uint32_t v)
{
    return ((v & 0x000000ffu) << 24) | ((v & 0x0000ff00u) << 8)
         | ((v & 0x00ff0000u) >> 8) | ((v & 0xff000000u) >> 24);
}

static int crc_matches(const uint8_t* data, int len, uint32_t key, int crcBytes)
{
    uint32_t crc = packet_crc(data, len, key);

    if (crcBytes == 2)
        return (uint16_t)((data[len] << 8) | data[len + 1]) == (uint16_t)crc;
    if (crcBytes == 4)
    {
        uint32_t want = ((uint32_t)data[len] << 24) | ((uint32_t)data[len + 1] << 16)
                      | ((uint32_t)data[len + 2] << 8) | data[len + 3];
        return want == crc;
    }

    return 0;
}

int packet_crc_strip(const uint8_t* data, int* len, uint32_t* key, int crcBytes)
{
    int l = *len;
    uint32_t swapped;

    if (crcBytes == 0 || packet_crc_exempt(data))
        return 1;

    /* Minimum is a 2-byte body (bare opcode) plus the checksum; a keepalive
     * (OP_KeepAlive, 0x06) is exactly 4 bytes on the wire when crcBytes is 2,
     * so this must be < and not <=. */
    if (l < crcBytes + 2)
        return 0;

    l -= crcBytes;

    if (crc_matches(data, l, *key, crcBytes))
    {
        *len = l;
        return 1;
    }

    /* Self-heal on key byte order: try the byte-swapped key once. */
    swapped = byteswap32(*key);
    if (crc_matches(data, l, swapped, crcBytes))
    {
        *key = swapped;
        *len = l;
        return 1;
    }

    return 0;
}
