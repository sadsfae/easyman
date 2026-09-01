
#ifndef _P99LOGIN_PROTOCOL_H_
#define _P99LOGIN_PROTOCOL_H_

#include "connection.h"
#include <stdint.h>

uint16_t get_protocol_opcode(uint8_t* data);
void recv_from_local(Connection* con, int len);
void recv_from_remote(Connection* con, uint8_t* data, int len);

#ifdef _DEBUG
void debug_write_packet(uint8_t* data, int len, int loginToClient);
#endif

#endif/*_P99LOGIN_PROTOCOL_H_*/
