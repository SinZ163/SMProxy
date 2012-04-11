SMProxy
=======

SMProxy is a tool for debugging protocol communication with a Minecraft server, currently supporting Minecraft 1.2.4.

SMProxy runs on Windows with Microsoft.NET and Mono, and on Linux/Mac with Mono.

Make sure that you use mono-complete on Linux/Mac.  For Linux with Aptitude, use "apt-get install mono-complete".  Use "yum install mono-complete" for Yum.

Compatability
-------------

If you are interested in using SMProxy for versions other than 1.2.4, check the /compatability directory for premade packet definitions for older versions of the protocol.

Usage
-----

Note: For Linux and Mac, you need to preface this with "mono".  Think of it like Java, but for CIL.

SMProxy.exe [flags] [server address]

The most basic usage of SMProxy is this:
SMProxy.exe [server address]

This will listen for connections on 127.0.0.1:25564 and log all communication to "output.txt" in the current directory.  When you connect a Minecraft client to that address, it will feed all communication through to [server address], and create a log of that communication in output.txt.  If at any time the client or server attempts to send data that is inconsistent with the 1.2.3 protocol, the proxy will degrade itself to a generic TCP proxy and output raw communication data.

Flags
-----

Flags may be used to customize the operation of SMProxy.  The following flags are available:

**Suppress client: -sc** (Alternate: --suppress-client)

Removes CLIENT->SERVER communication from the log.

**Suppress server: -ss** (Alternate: --suppress-server)

Removes SERVER->CLIENT communication from the log.

**Enable profiling: -pr** (Alternate: --enable-profiling)

Will ouput information about timing to the log.

**Output: -o [file]** (Alternate: --ouput)

Outputs a communication log to [file].  Default: "output.txt"

**Port: -p [port]** (Alternate: --port)

Listens on the specified port for incoming traffic.  Default: 25564

**Filter: -f [filter]** (Alternate: --filter)

Filters which packets are logged.  This is a comma-delimited list of packets IDs, in hexadecimal.

Example usage: "-f 00,03,04" will filter output to only show keep-alives, chat messages, and time updates.

**!Not Filter: -!f [filter]** (Alternate: --!filter)

The opposite of -f, packets listed here will be ommitted from the output.

**Suppress packet: -sp [packet]:[direction],...** (Alternate: --suppress-packet)

Unlike -sc and -ss, this will suppress an individual packet from being reported to either the client or server, or both.  -ss and -sc affect the log only, where -sp will affect actual communication.  [packet] is a packet ID, in hexadecimal.  [direction] is a combination of the characters 'C' and 'S', representing which endpoint will be denied these packets.  This flag accepts a comma delimited list of these entries.

Example usage: "-sp 03:C" will prevent the client from recieving chat message packets.  "-sp 12:CS,65:S" will prevent any transmission of animation packets, as well as prevent the server from recieving any window close packets from the client.

**Add packet: -ap [name]:[id]:[direction]:[structure]** (Alternate: --add-packet)

This flag may be used to add an additional packet to the internal protocol implementation.  This can be useful for testing custom packets, or new versions of the protocol.  Custom packets will also override the existing implementation of the specified packet, if an implementation exists.  [name] is the name of the packet, as it should appear in the log.  Do not use spaces.  [id] is the packet ID, in hexadecimal.  [direction] represents which direction of communication is valid for this packet, a combination of the characters 'C' and 'S', representing which endpoint may send this packet.  [structure] is a comma-delimited list of data types found in this packet.

Valid data types are *byte, short, int, long, float, double, string, mob, slot*, and *array*.  They represent, in order, how the packet is to be read.

Example usage:

    -ap ChatMessage:03:CS:string
	
This would re-define the 1.2.3 chat message packet.

    -ap EntityEquipment:05:S:int,short,short,short

This would re-define the 1.2.3 entity equipment packet.

You may also use named values and arrays.  Here's an example of how you could implement the Map Chunks packet from the 1.2.3 protocol, using named values and dynamic arrays:

    -ap MapChunks:33:S:int(X),int(Z),boolean(GroundUpContiguous),short(PrimaryBitMap),short(AddBitMap),int(CompressedSize),int,array(CompressedData)[CompressedSize]
	
You are allowed to reference any value (by name) that you had previously referenced.  Do not use spaces in value names or array expressions.  You may also use mathematical expresions in arrays.  For example, "array[(valueByName*3)/5]" would be valid.  The final value will be converted to an integer after the calculation is complete.  You may also use a variety of functions, such as Min and Sqrt.  Another valid example would be "array[Min(value1,value2)]".  A list of these functions is available online.  It includes every function in the .NET class System.Math: http://msdn.microsoft.com/en-us/library/system.math.aspx

**Parameter File: -pf [file]** (Alternate: --packet-file)

This will load additional parameters from the disk.  It consists of a series of command-line parameters on their own lines, with "#"-prefaced comments and leading and trailing whitespace allowed.

**Protocol Version: -pv [version]** (Alternate: --protocol-version)

Changes the protocol used in packet 0x01.  [version] is in decimal.  The default value is 29.

**Endpoint: -ep [endpoint]** (Alternate: --endpoint)

Changes the local endpoint.  To listen on all interfaces, use "0.0.0.0".  The default value is "127.0.0.1".

**Suppress Log: -sl** (Alternate: --suppress-log)

Completely stops any log files from being produced.

**Persistent Sessions: -ps** (Alternate: --persistent-session)

Changes to persistent session mode.  The default behavior is to handle one session, then exit.  Enabling this will continue to idle and accept connections over time, until "quit" is typed into the console.  This will also allow multiple clients to connect simulataneously.

**Import Script: -is [packet name]:[packet id]:[direction]:[script file]** (Alternate: --import-script)

Imports a Lua script to be used for the specified packet.  Packet ID is in hexadecimal.  The direction is a combination of the characters 'C' and 'S', representing which side of the protocol may send the packet.  See LUA.md for more information on writing packet scripts.

**Virtual Host: -vh [host]:[destination]** (Alternate: --virtual-host)

Adds a virtual host to SMProxy.  Any connections to SMProxy via [host] will be redirected to [destination].  A default endpoint is still required as the last argument when starting SMProxy.

Packet Logs
-----------

An example packet entry could look like this:

    {6:23:31 PM} [SERVER->CLIENT]: LoginRequest (0x1)
        [00:00:00:00:00:00:00:04:00:66:00:6c:00:61:00:74:00:00:00:01:00:00:00:00:01:00:14]
        Protocol Version (Int32): 0
        [unused] (String): 
        Level Type (String): flat
        Server Mode (Int32): 1
        Dimension (Int32): 0
        Difficulty (Byte): 1
        World Height (Byte): 0
        Max Players (Byte): 20
        
There's a lot of information available here.  The first line is this:

    {6:23:31 PM} [SERVER->CLIENT]: LoginRequest (0x1)

Between the { } brackets is the time that this packet was logged.  The [ ] brackets show the direction of communication.  Next is the friendly name of the packet, followed by it's hexadecimal ID in parenthesis.

When a custom packet is used (via -ap), the value in the brackets will specify that it is a custom packet (example: "[CUSTOM SERVER->CLIENT]").  The same holds true for packets suppressed via -sp.

    [00:00:00:00:00:00:00:04:00:66:00:6c:00:61:00:74:00:00:00:01:00:00:00:00:01:00:14]
    
Next is a dump of the raw packet contents, in hexadecimal.

This is followed by a series of entries.  Each of these items maps to how SMProxy interprets each packet's contents.

    Protocol Version (Int32): 0
    
First, you have the friendly name.  This is followed by the value type in parenthesis, and finally the value of the entry.

If SMProxy is unable to interpret the protocol properly (perhaps caused by a difference in protocol versions), it will revert to a generic TCP proxy and log each byte transferred in hexadecimal.  It will only do this on a per-endpoint basis - if the client sends invalid protocol, but the server does not, the server logs will continue to be detailed.

Building from Source
--------------------

If you'd like to build SMProxy from source, there are several ways to go about it.

**Windows**

Make sure you have .NET 4.0 installed.  Then, add "C:\Windows\Microsoft.NET\Framework64\v4.0.30319" to your path.  Run "msbuild" from /SMProxy to build SMProxy.

You can also open and edit the solution with Visual Studio 2010 or better, and SharpDevelop 4.0 or better.

**Linux**

Install the "mono-complete" package.  Then, run "xbuild" from /SMProxy to build SMProxy.

You can also open and edit the solution with MonoDevelop 2.8 or better.