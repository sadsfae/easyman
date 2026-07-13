
#ifndef _P99LOGIN_ALLOWED_H_
#define _P99LOGIN_ALLOWED_H_

#define MAX_ALLOWED_SERVERS 64
#define MAX_SERVER_NAME_LEN 128

typedef struct AllowedServers {
    char names[MAX_ALLOWED_SERVERS][MAX_SERVER_NAME_LEN];
    int count;
} AllowedServers;

extern AllowedServers g_allowed;

int allowed_load(AllowedServers* allowed, const char* filepath);
int allowed_match(const AllowedServers* allowed, const char* name);

#endif/*_P99LOGIN_ALLOWED_H_*/
