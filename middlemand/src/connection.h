
#ifndef _P99LOGIN_CONNECTION_H_
#define _P99LOGIN_CONNECTION_H_

#include "netcode.h"
#include "sequence.h"
#include "errors.h"
#include <stdint.h>
#include <string.h>
#include <stdio.h>
#include <setjmp.h>
#include <time.h>

#ifndef MIDDLEMAN_PORT
#define MIDDLEMAN_PORT 5998
#endif

#ifndef REMOTE_HOST
#define REMOTE_HOST "login.eqemulator.net"
#endif

#ifndef REMOTE_PORT
#define REMOTE_PORT "5998"
#endif

#define BUFFER_SIZE 2048
#define SESSION_TIMEOUT_SECONDS 60

typedef struct sockaddr_in Address;

typedef struct Connection {
    int socket;         /* Client-facing socket, bound to MIDDLEMAN_PORT */
    int remoteSocket;   /* Per-login-attempt upstream socket (fresh ephemeral
                           endpoint every session; INVALID_SOCKET when idle) */
    int inSession;
    time_t lastRecvTime;
    Address localAddr;
    Address remoteAddr;

    /* Packet-CRC contract adopted from the last OP_SessionResponse. */
    int crcBytes;       /* 0, 2 or 4 */
    uint32_t crcKey;    /* session encode key (host order; fed LE to the CRC) */

    jmp_buf jmpBuf;
    uint8_t buffer[BUFFER_SIZE];
    /* Scratch buffer for egress datagrams that carry an appended CRC, so the
       checksum never has to be written over live receive-buffer data (which
       would clobber adjacent OP_Combined sub-packets). */
    uint8_t txBuf[BUFFER_SIZE + 4];
    Sequence sequence;
} Connection;

void connection_open(Connection* con);
void connection_close(Connection* con);
void connection_read(Connection* con);
void connection_send(Connection* con, void* data, int len, int toRemote);
void connection_reset(Connection* con, Address* addr);
void connection_upstream_open(Connection* con);

#endif/*_P99LOGIN_CONNECTION_H_*/
