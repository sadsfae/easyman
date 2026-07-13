# Override with: make CC=i686-w64-mingw32-gcc (or set in MSYS2 MINGW32 env)
CC = i686-w64-mingw32-gcc
CFLAGS = -O2 -Wall -Wno-strict-aliasing
LDFLAGS = -lws2_32
BINDIR = ./bin/
SRCDIR = ./src/
OBJDIR = ./build/

_OBJS = main.o connection.o protocol.o sequence.o allowed.o
_DEPS = connection.h protocol.h sequence.h netcode.h errors.h allowed.h

OBJS = $(patsubst %,$(OBJDIR)%,$(_OBJS))
DEPS = $(patsubst %,$(SRCDIR)%,$(_DEPS))

default all: $(BINDIR)middleman.exe

$(BINDIR)middleman.exe: $(OBJS) | $(BINDIR)
	$(CC) -o $@ $^ $(CFLAGS) $(LDFLAGS)

$(OBJDIR)%.o: $(SRCDIR)%.c $(DEPS) | $(OBJDIR)
	$(CC) -c -o $@ $< $(CFLAGS)

$(BINDIR) $(OBJDIR):
	mkdir -p $@

.PHONY: clean
clean:
	rm -f $(OBJDIR)*.o $(BINDIR)middleman.exe
