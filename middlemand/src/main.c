
#include "connection.h"
#include "allowed.h"
#include <string.h>

AllowedServers g_allowed;

int print_error(int errcode);

int main(int argc, char* argv[])
{
    Connection con;
    int exception;
    char configPath[260];
    char* lastSlash;

    (void)argc;

#ifdef _WIN32
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa))
        return print_error(ERR_WSASTARTUP);
#endif

    strncpy(configPath, argv[0], sizeof(configPath) - 1);
    configPath[sizeof(configPath) - 1] = '\0';
    lastSlash = strrchr(configPath, '\\');
    if (!lastSlash)
        lastSlash = strrchr(configPath, '/');
    if (lastSlash)
        *(lastSlash + 1) = '\0';
    else
        configPath[0] = '\0';
    strncat(configPath, "allowed_emu.txt", sizeof(configPath) - strlen(configPath) - 1);
    allowed_load(&g_allowed, configPath);

    exception = setjmp(con.jmpBuf);

    if (exception)
    {
        connection_close(&con);
        return print_error(exception);
    }

    connection_open(&con);

    for (;;)
    {
        /* This always blocks */
        connection_read(&con);
    }

    connection_close(&con);

    return 0;
}

int print_error(int errcode)
{
    printf("Error: %d\n", errcode);
    return errcode;
}
