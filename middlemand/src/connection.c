
#include "connection.h"
#include "protocol.h"
#include "crc.h"

static int resolve_remote(Connection* con)
{
    struct addrinfo hints;
    struct addrinfo* remote;

    memset(&hints, 0, sizeof(struct addrinfo));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_DGRAM;

    if (getaddrinfo(REMOTE_HOST, REMOTE_PORT, &hints, &remote))
        return 0;

    con->remoteAddr = *(Address*)remote->ai_addr;

    freeaddrinfo(remote);
    return 1;
}

/* Open a fresh upstream socket to the login server.  Every login attempt
 * (triggered by connection_reset) gets a NEW ephemeral local endpoint: the
 * login server tracks a session per source endpoint and ignores a session
 * request from an endpoint it already has one for, so a proxy that reused a
 * single fixed socket would hang on every relog/retry.  This matches the
 * working Go port (middlemand-standalone.go). */
void connection_upstream_open(Connection* con)
{
    /* Close the previous upstream first; anything still in flight to it was
       for the previous attempt. */
    if (con->remoteSocket != INVALID_SOCKET)
    {
        closesocket(con->remoteSocket);
        con->remoteSocket = INVALID_SOCKET;
    }

    /* Re-resolve per attempt (also heals a login-server IP change without a
     * proxy restart); on failure keep the last known address rather than
     * killing the proxy over a transient DNS problem. */
    resolve_remote(con);

    con->remoteSocket = socket(AF_INET, SOCK_DGRAM, 0);
    if (con->remoteSocket == INVALID_SOCKET)
        longjmp(con->jmpBuf, ERR_SOCKET_CALL);
}

void connection_open(Connection* con)
{
    Address addr;

    /* Do this now so we won't segfault if we longjmp from an error below */
    sequence_init(con);

    con->remoteSocket = INVALID_SOCKET;
    con->inSession = 0;
    con->lastRecvTime = 0;
    con->crcBytes = 0;
    con->crcKey = 0;

    /* Resolve the login server once so we always have a usable address;
     * per-attempt re-resolution in upstream_open is best-effort. */
    if (!resolve_remote(con))
        longjmp(con->jmpBuf, ERR_GETADDRINFO_CALL);

    /* Create the client-facing socket.  The EQ client sends to localhost. */
    con->socket = socket(AF_INET, SOCK_DGRAM, 0);
    if (con->socket == INVALID_SOCKET)
        longjmp(con->jmpBuf, ERR_SOCKET_CALL);

    memset(&addr, 0, sizeof(Address));
    addr.sin_family = AF_INET;
    addr.sin_port = ToNetworkShort(MIDDLEMAN_PORT);
    addr.sin_addr.s_addr = ToNetworkLong(INADDR_ANY);

    if (bind(con->socket, (struct sockaddr*)&addr, sizeof(Address)))
        longjmp(con->jmpBuf, ERR_BIND_CALL);
}

void connection_close(Connection* con)
{
    if (con->socket != INVALID_SOCKET)
    {
        closesocket(con->socket);
        con->socket = INVALID_SOCKET;
    }

    if (con->remoteSocket != INVALID_SOCKET)
    {
        closesocket(con->remoteSocket);
        con->remoteSocket = INVALID_SOCKET;
    }

    sequence_free(con);

#ifdef _WIN32
    WSACleanup();
#endif
}

static void connection_read_local(Connection* con)
{
    Address addr;
    socklen_t addrLen = sizeof(Address);
    int len;
    time_t recvTime;

    len = recvfrom(con->socket, (char*)con->buffer, BUFFER_SIZE, 0, (struct sockaddr*)&addr, &addrLen);

    if (len < 2)
    {
        if (len == -1)
        {
#ifdef _WIN32
            if (WSAGetLastError() != WSAECONNRESET && WSAGetLastError() != WSAEINTR)
                longjmp(con->jmpBuf, ERR_RECVFROM);
#else
            if (errno != EWOULDBLOCK && errno != EAGAIN && errno != ESHUTDOWN && errno != EINTR)
                longjmp(con->jmpBuf, ERR_RECVFROM);
#endif
        }
        return;
    }

    recvTime = time(NULL);

    /* Start serving this client (and open a fresh upstream) when it opens a
     * session, when we have none, or when the previous one went quiet.  Every
     * OP_SessionRequest gets a NEW upstream socket, deliberately including a
     * retry of one we already forwarded: the login server ignores a session
     * request from an endpoint it already has a session for. */
    if (get_protocol_opcode(con->buffer) == 0x01
        || !con->inSession
        || (recvTime - con->lastRecvTime) > SESSION_TIMEOUT_SECONDS)
        connection_reset(con, &addr);

    /* Every new attempt negotiates its own CRC state via its
     * SessionResponse; here everything inside con->buffer is CRC-stripped
     * (length reduced) so downstream code never sees checksums. */
    if (!packet_crc_strip(con->buffer, &len, &con->crcKey, con->crcBytes))
        return; /* bad checksum from the client — drop */

    recv_from_local(con, len);
    con->lastRecvTime = recvTime;
}

static void connection_read_remote(Connection* con)
{
    int len;
    time_t recvTime;

    len = recvfrom(con->remoteSocket, (char*)con->buffer, BUFFER_SIZE, 0, NULL, NULL);

    if (len < 2)
    {
        if (len == -1)
        {
#ifdef _WIN32
            if (WSAGetLastError() != WSAECONNRESET && WSAGetLastError() != WSAEINTR)
                longjmp(con->jmpBuf, ERR_RECVFROM);
#else
            if (errno != EWOULDBLOCK && errno != EAGAIN && errno != ESHUTDOWN && errno != EINTR)
                longjmp(con->jmpBuf, ERR_RECVFROM);
#endif
        }
        return;
    }

    recvTime = time(NULL);

    if (!packet_crc_strip(con->buffer, &len, &con->crcKey, con->crcBytes))
        return; /* bad checksum from the server — drop */

    recv_from_remote(con, con->buffer, len);
    con->lastRecvTime = recvTime;
}

void connection_read(Connection* con)
{
    for (;;)
    {
        fd_set rfds;
        int maxfd = con->socket;

        FD_ZERO(&rfds);
        FD_SET(con->socket, &rfds);

        if (con->remoteSocket != INVALID_SOCKET)
        {
            FD_SET(con->remoteSocket, &rfds);
            if (con->remoteSocket > maxfd)
                maxfd = con->remoteSocket;
        }

        if (select(maxfd + 1, &rfds, NULL, NULL, NULL) < 0)
        {
#ifdef _WIN32
            if (WSAGetLastError() != WSAEINTR)
                longjmp(con->jmpBuf, ERR_SELECT);
#else
            if (errno != EINTR)
                longjmp(con->jmpBuf, ERR_SELECT);
#endif
            continue;
        }

        if (FD_ISSET(con->socket, &rfds))
            connection_read_local(con);

        if (con->remoteSocket != INVALID_SOCKET && FD_ISSET(con->remoteSocket, &rfds))
            connection_read_remote(con);
    }
}

void connection_send(Connection* con, void* data, int len, int toRemote)
{
    Address* addr;
    int sock;
    uint8_t* out = (uint8_t*)data;
    int outLen = len;
    int sent;

    if (toRemote)
    {
        if (con->remoteSocket == INVALID_SOCKET)
            return; /* No upstream yet — nothing to forward to. */
        sock = con->remoteSocket;
        addr = &con->remoteAddr;
    }
    else
    {
        sock = con->socket;
        addr = &con->localAddr;
    }

    /* Recompute the checksum on every egress datagram: everything downstream
     * works on CRC-stripped bytes (and rewrites sequences), so any checksum
     * that came in with the packet is stale by now.  Session-negotiation
     * opcodes are exempt.  Fits in txBuf whenever len <= BUFFER_SIZE; the
     * one caller that can exceed that (the rebuilt server list) appends the
     * CRC itself before calling us. */
    if (con->crcBytes != 0 && !packet_crc_exempt((uint8_t*)data))
    {
        uint32_t crc = packet_crc((uint8_t*)data, len, con->crcKey);

        if (len + con->crcBytes <= (int)sizeof(con->txBuf))
        {
            memcpy(con->txBuf, data, len);
            out = con->txBuf;
            if (con->crcBytes == 2)
            {
                con->txBuf[len] = (uint8_t)(crc >> 8);
                con->txBuf[len + 1] = (uint8_t)(crc & 0xff);
                outLen = len + 2;
            }
            else if (con->crcBytes == 4)
            {
                con->txBuf[len] = (uint8_t)(crc >> 24);
                con->txBuf[len + 1] = (uint8_t)(crc >> 16);
                con->txBuf[len + 2] = (uint8_t)(crc >> 8);
                con->txBuf[len + 3] = (uint8_t)crc;
                outLen = len + 4;
            }
        }
    }

#ifdef _DEBUG
    debug_write_packet(out, outLen, !toRemote);
#endif
    sent = sendto(sock, (char*)out, outLen, 0, (struct sockaddr*)addr, sizeof(Address));

    if (sent == -1)
        longjmp(con->jmpBuf, ERR_SENDTO);
}

/* Begin a new client session: record where the client talks from, drop all
 * reassembly state, and open a fresh upstream socket for this attempt. */
void connection_reset(Connection* con, Address* addr)
{
    con->localAddr = *addr;
    con->inSession = 0;
    con->crcBytes = 0;
    con->crcKey = 0;
    sequence_free(con);
    connection_upstream_open(con);
}
