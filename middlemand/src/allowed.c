
#include "allowed.h"
#include <stdio.h>
#include <string.h>
#include <ctype.h>

static void str_tolower(char* dst, const char* src, int maxlen)
{
    int i;
    for (i = 0; i < maxlen - 1 && src[i]; i++)
        dst[i] = (char)tolower((unsigned char)src[i]);
    dst[i] = '\0';
}

static char* trim(char* s)
{
    int len;
    char* start = s;
    while (*start == ' ' || *start == '\t')
        start++;
    if (start != s)
        memmove(s, start, strlen(start) + 1);
    len = (int)strlen(s);
    while (len > 0 && (s[len - 1] == '\r' || s[len - 1] == '\n' ||
           s[len - 1] == ' ' || s[len - 1] == '\t'))
        s[--len] = '\0';
    return s;
}

int allowed_load(AllowedServers* allowed, const char* filepath)
{
    FILE* f;
    char line[MAX_SERVER_NAME_LEN];

    allowed->count = 0;

    f = fopen(filepath, "r");
    if (!f)
        return -1;

    while (fgets(line, sizeof(line), f) && allowed->count < MAX_ALLOWED_SERVERS)
    {
        trim(line);

        if (line[0] == '\0' || line[0] == '#' || line[0] == ';')
            continue;

        str_tolower(allowed->names[allowed->count], line, MAX_SERVER_NAME_LEN);
        allowed->count++;
    }

    fclose(f);
    return 0;
}

int allowed_match(const AllowedServers* allowed, const char* name)
{
    char lower_name[MAX_SERVER_NAME_LEN];
    int i;

    str_tolower(lower_name, name, MAX_SERVER_NAME_LEN);

    for (i = 0; i < allowed->count; i++)
    {
        if (strstr(lower_name, allowed->names[i]))
            return 1;
    }

    return 0;
}
