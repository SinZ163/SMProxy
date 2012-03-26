SMProxy with Lua
======

You can specify custom packet implementations with Lua as of SMProxy version 1.2.4.1.  Each packet must have one script file associated with it.  You may import these files via the "-is", or "--import-script" parameter.

SMProxy exposes a number of functions that a Lua script may use.  They each read from the connected socket and output the resulting value, both returning it to the caller and sending it to the connected socket.

Lua is not currently supported on Mono.

They are:

* readByte()
* readBytes(int)
* readBoolean()
* readShort()
* readInt()
* readLong()
* readString()
* readMob()
* readSlot()

A very simple script, to re-implement the 0x1F, or "Entity Relative Move" packet:

    readInt()
    readByte()
    readByte()
    readByte()