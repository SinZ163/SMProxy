SMProxy
======

SMProxy is a tool for debugging protocol communication with a Minecraft server, currently supporting Minecraft 1.2.3.

SMProxy runs on Windows with Microsoft.NET and Mono, and on Linux/Mac with Mono.


Make sure that you use mono-complete on Linux/Mac.  For Linux with Aptitude, use "apt-get install mono-complete".  Use "yum install mono-complete" for Yum.

Usage
-----------

Note: For Linux and Mac, you need to preface this with "mono".  Think of it like Java, but for CIL.

SMProxy.exe [flags] [server address]

The most basic usage of SMProxy is this:
SMProxy.exe [server address]

This will listen for connections on 127.0.0.1:25564 and log all communication to "output.txt" in the current directory.  When you connect a Minecraft client to that address, it will feed all communication through to [server address], and create a log of that communication in output.txt.  If at any time the client or server attempts to send data that is inconsistent with the 1.2.3 protocol, the proxy will degrade itself to a generic TCP proxy and output raw communication data.

Compatability
-----------

If you are interested in using SMProxy for versions other than 1.2.3, check the /compatability directory for premade packet definitions for older versions of the protocol.

Flags
-----------

Flags may be used to customize the operation of SMProxy.  The following flags are available:

**Suppress client: -sc**

Removes CLIENT->SERVER communication from the log.

**Suppress server: -ss**

Removes SERVER->CLIENT communication from the log.

**Enable profiling: -ep**

Will ouput information about timing to the log.

Example output: "Profiling: Size: 2246; down: 8.0005 ms (280732.454221611 bytes/sec); up: 7.0004 ms (320838.809210902 bytes/sec); Proxy lag: 23.0014 ms"

**Output: -o [file]**

Outputs a communication log to [file].  Default: "output.txt"

**Port: -p [port]**

Listens on the specified port for incoming traffic.  Default: 25564

**Filter: -f [filter]**

Filters which packets are logged.  This is a comma-delimited list of packets IDs, in hexadecimal.

Example usage: "-f 00,03,04" will filter output to only show keep-alives, chat messages, and time updates.

**!Not Filter: -!f [filter]**

The opposite of -f, packets listed here will be ommitted from the output.

**Suppress packet: -sp [packet]:[direction],...**

Unlike -sc and -ss, this will suppress an individual packet from being reported to either the client or server, or both.  -ss and -sc affect the log only, where -sp will affect actually communication.  [packet] is a packet ID, in hexadecimal.  [direction] is a combination of the characters 'C' and 'S', representing which endpoint will be denied these packets.  This flag accepts a comma delimited list of these entries.

Example usage: "-sp 03:C" will prevent the client from recieving chat message packets.  "-sp 12:CS,65:S" will prevent any transmission of animation packets, as well as prevent the server from recieving any window close packets from the client.

**Add packet: -ap [name]:[id]:[direction]:[structure]**

This flag may be used to add an additional packet to the internal protocol implementation.  This can be useful for testing custom packets, or new versions of the protocol.  Custom packets will also override the existing implementation of the specified packet, if an implementation exists.  [name] is the name of the packet, as it should appear in the log.  Do not use spaces.  [id] is the packet ID, in hexadecimal.  [direction] represents which direction of communication is valid for this packet, a combination of the characters 'C' and 'S', representing which endpoint may send this packet.  [structure] is a comma-delimited list of data types found in this packet.

Valid data types are byte, short, int, long, float, double, string, mob, slot, and array.  They represent, in order, how the packet is to be read.

Example usage: "-ap ChatMessage:03:CS:string" would re-define the 1.2.3 chat message packet.  "-ap EntityEquipment:05:S:int,short,short,short" would re-define the 1.2.3 entity equipment packet.

You may also use named values and arrays.  Here's an example of how you could implement the Map Chunks packet from the 1.2.3 protocol, using named values and dynamic arrays:

    -ap MapChunks:33:S:int(X),int(Z),boolean(GroundUpContiguous),short(PrimaryBitMap),short(AddBitMap),int(CompressedSize),int,array(CompressedData)[CompressedSize]
	
You are allowed to reference any value (by name) that you had previously referenced.  Do not use spaces in value names or array expressions.  You may also use mathematical expresions in arrays.  For example, "array[(valueByName*3)/5]" would be valid.  The final value will be converted to an integer after the calculation is complete.  You may also use a variety of functions, such as Min and Sqrt.  Another valid example would be "array[Min(value1,value2)]".  A list of these functions is available online.  It includes every function in the .NET class System.Math: http://msdn.microsoft.com/en-us/library/system.math.aspx

**Add packet file: -ap [file]**

This form of -ap will load a packet definition file from the disk.  It consists of a series of packet definitions on their own lines, with "#"-prefaced comments and leading and trailing whitespace allowed.