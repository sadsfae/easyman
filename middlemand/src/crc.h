#ifndef _P99LOGIN_CRC_H_
#define _P99LOGIN_CRC_H_

#include <stdint.h>

/* Session-negotiation opcodes never carry a CRC (EQEmu PacketCanBeEncoded):
 * OP_SessionRequest, OP_SessionResponse, OP_OutOfSession. */
int packet_crc_exempt(const uint8_t* data);

/* EQEmu login-stream checksum: standard CRC-32 (reflected poly 0xEDB88320)
 * fed the 4-byte session key (little-endian) then the packet bytes, i.e.
 * the zlib-style crc32 over key bytes + data.  This is exactly what the Go
 * port computes with crc32.Update chains and what EQEmu's
 * EQ::Crc32(data, size, key) returns; verified live against the P99 login
 * server (a packet carrying this checksum is accepted, a corrupted one is
 * dropped). */
uint32_t packet_crc(const uint8_t* data, int len, uint32_t key);

/* Validate and strip the trailing checksum from an ingress datagram.
 * On success returns 1 with *len reduced by crcBytes.  If validation fails,
 * the byte-swapped key is tried once and adopted on success (self-healing,
 * mirrors the Go port).  Returns 0 to drop the datagram. */
int packet_crc_strip(const uint8_t* data, int* len, uint32_t* key, int crcBytes);

#endif/*_P99LOGIN_CRC_H_*/
