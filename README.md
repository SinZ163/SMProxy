SMProxy
=======

SMProxy is a tool for debugging protocol communication with a Minecraft server, currently supporting Minecraft 12w41a.

SMProxy runs on Windows with Microsoft.NET and Mono, and on Linux/Mac with Mono.

Make sure that you use mono-complete on Linux/Mac.  For Linux with Aptitude, use "apt-get install mono-complete".  Use "yum
install mono-complete" for Yum.

Usage
-----

Note: For Linux and Mac, you need to preface this with "mono".  Think of it like Java, but for CIL.

SMProxy.exe [flags] [server address]

The most basic usage of SMProxy is this:
SMProxy.exe

This will listen for connections on 127.0.0.1:25564 and log all communication to "output.txt" in the current directory.  When
you connect a Minecraft client to that address, it will feed all communication through to 127.0.0.1:25565, and create a log
of that communication in output.txt.  If at any time the client or server attempts to send data that is inconsistent with the
12w41a protocol, the proxy will degrade itself to a generic TCP proxy and output raw communication data.

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

Outputs a communication log to [file].  The default is based on the connecting client and the current time.

**Port: --port [port]**

Listens on the specified port for incoming traffic.  Default: 25564

**Filter: -f [filter]** (Alternate: --filter)

Filters which packets are logged.  This is a comma-delimited list of packets IDs, in hexadecimal.

Example usage: "-f 00,03,04" will filter output to only show keep-alives, chat messages, and time updates.

**!Not Filter: -!f [filter]** (Alternate: --!filter)

The opposite of -f, packets listed here will be ommitted from the output.

**Suppress packet: -sp [packet]:[direction],...** (Alternate: --suppress-packet)

Unlike -sc and -ss, this will suppress an individual packet from being reported to either the client or server, or both.
-ss and -sc affect the log only, where -sp will affect actual communication.  [packet] is a packet ID, in hexadecimal. 
[direction] is a combination of the characters 'C' and 'S', representing which endpoint will be denied these packets. 
This flag accepts a comma delimited list of these entries.

Example usage: "-sp 03:C" will prevent the client from recieving chat message packets.  "-sp 12:CS,65:S" will prevent
any transmission of animation packets, as well as prevent the server from recieving any window close packets from the
client.

**Endpoint: -ep [endpoint]** (Alternate: --endpoint)

Changes the local endpoint.  To listen on all interfaces, use "0.0.0.0".  The default value is "127.0.0.1:25565".

**Persistent Sessions: -ps** (Alternate: --persistent-session)

Changes to persistent session mode.  The default behavior is to handle one session, then exit.  Enabling this will continue
to idle and accept connections over time.  This will also allow multiple clients to connect simulataneously.

**Username: -u [username]** (Alternate: --username)

Specifies the username to use when logging into online mode servers. If you do not provide this, SMProxy will attempt to
decrypt a local lastlogin file from ~/.minecraft. If all of this fails, SMProxy will not work with online mode servers.

**Password: -p [password]**: (Alternate: --password)

Specifies the password to use when logging into online mode servers. If you do not provide this, SMProxy will prompt you.
If you do not specify --username, SMProxy will ignore this and attempt to decrypt lastlogin.

Packet Logs
-----------

At the start of the log is a section like this:

	Log opened on Saturday, September 08, 2012 at 5:08:58 PM
	Settings:
	Local endpoint: 127.0.0.1:25564
	Remote endpoint: 127.0.0.1:25565
	Single session: True
	Log client traffic: True
	Log server traffic: True
	Profiling enabled: False

It gives details about the session configuration at the time the log was created.

An example packet entry could look like this:

	{5:03:39 PM} [SERVER->CLIENT 127.0.0.1:54620]: Login Request (0x01)
		[
			01 00 00 00 1A 00 04 00 66 00 6C 00 61 00 74 01     . . . . . . . . f . l . a . t . 
			00 01 00 14                                         . . . . 
		]
		Entity Id (Int32): 26
		Level Type (String): flat
		Game Mode (Byte): Creative
		Dimension (Byte): Overworld
		Difficulty (Byte): Easy
		Max Players (Byte): 20
		[discarded] (Byte): 0
        
There's a lot of information available here.  The first line is this:

    {5:03:39 PM} [SERVER->CLIENT 127.0.0.1:54620]: Login Request (0x01)

Between the { } brackets is the time that this packet was logged.  The [ ] brackets show the direction of communication.  Next is the friendly name
of the packet, followed by it's hexadecimal ID in parenthesis.

	[
		01 00 00 00 1A 00 04 00 66 00 6C 00 61 00 74 01     . . . . . . . . f . l . a . t . 
		00 01 00 14                                         . . . . 
	]
    
Next is a dump of the raw packet contents, in hexadecimal.  Each row is 16 octects (bytes) wide, and the text on the right is the ASCII representation
of each. If the character is not a letter or digit, '.' is displayed instead.

This is followed by a series of entries.  Each of these items maps to how SMProxy interprets each packet's contents.

    Entity Id (Int32): 26
    
First, you have the friendly name.  This is followed by the value type in parenthesis, and finally the value of the entry.

If SMProxy is unable to interpret the protocol properly (perhaps caused by a difference in protocol versions), it will revert to a generic TCP proxy
and log each byte transferred in hexadecimal.  It will only do this on a per-endpoint basis - if the client sends invalid protocol, but the server does
not, the server logs will continue to be detailed.

Building from Source
--------------------

If you'd like to build SMProxy from source, there are several ways to go about it.

**Windows**

Make sure you have .NET 4.0 installed.  Then, add "C:\Windows\Microsoft.NET\Framework64\v4.0.30319" to your path.  Run "msbuild" from /SMProxy to build SMProxy.

You can also open and edit the solution with Visual Studio 2010 or better, and SharpDevelop 4.0 or better.

**Linux**

Install the "mono-complete" package.  Then, run "xbuild" from /SMProxy to build SMProxy.

You can also open and edit the solution with MonoDevelop 2.8 or better.
