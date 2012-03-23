using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibMinecraft.Model;
using NCalc;

namespace SMProxy
{
    static partial class Program
    {
        static TcpListener Listener;
        static bool ClientDirty = false, ServerDirty = false;
        static bool SuppressServer = false, SuppressClient = false, FilterOutput = false,
            IgnoreFilterOutput = false, EnableProfiling = false, SuppressLog = false, PersistentSessions = false;
        static string OutputFile = "output.txt", ServerAddress = null;
        static List<byte> Filter = new List<byte>();
        static List<byte> IgnoreFilter = new List<byte>();
        static List<byte> ClientDenyPackets = new List<byte>();
        static List<byte> ServerDenyPackets = new List<byte>();
        static int LocalPort = 25564, RemotePort = 25565;
        static int ProtocolVersion = 29;
        static string LocalEndpoint = "127.0.0.1";
        static Dictionary<byte, string> CustomClientPackets = new Dictionary<byte, string>();
        static Dictionary<byte, string> CustomServerPackets = new Dictionary<byte, string>();
        static StreamWriter outputLogger = null;

        static void DisplayHelp()
        {
            Console.WriteLine("Usage: SMProxy.exe [flags] [server address]\n" +
                "Flags:\n" +
                "-o [file]: Set output to [file].  Default: output.txt\n\tAlternate: --output\n" +
                "-p [port]: Set local endpoint to listen on [port].  Default: 25564\n\tAlternate: --port\n" +
                "-f [filter]: Logs only packets that match [filter].\n\tAlternate: --filter\n" +
                "\t[filter] is a comma-delimited list of packet IDs, in hex.\n" +
                "\tExample: -f 01,02 would restrict output to handshake and login packets.\n" +
                "\tAlternatively, you can do -!f [filter] for the opposite behavior.\n" +
                "-sc: Suppress client.  Suppresses logging for client->server packets.\n\tAlternate: --suppress-client\n" +
                "-ss: Suppress server.  Suppresses logging for server->client packets.\n\tAlternate: --suppress-server\n" +
                "-pr: Enable profiling.  Logs speed of transmission.\n\tAlternate: --enable-profiling\n" +
                "-ap [packet]: Adds an additional packet.  This can prevent packet handling\n" +
                "\terrors when testing new versions of the Minecraft protocol.\n\n" +
                "\tPacket Format: [name]:[id (hex)]:[direction]:[values]\n" +
                "\tDirection is one or both of the characters 'C' and 'S', for each\n" + 
                "\tside of the connection that is sending the packet.\n" +
                "\tExample for the pre-existing entity equipment packet:\n" +
                "\t-ap EntityEquipment:S:05:int,short,short,short\n" +
                "\tDo not use spaces in the packet name.\n" +
                "\tValid data types:\n\tboolean, byte, short, int, long, float, double, string, mob, slot,\n\tarray[count]\n" +
                "\tYou may also specify names for each item.  Example: long(name).  You\n" +
                "\tcan also use this name as a parameter to an array.  For instance,\n" +
                "\t\"int(example),array[example]\".  You may do basic math in this\n" +
                "\tstatement: array[example*4].  Do not use spaces.\n" +
                "\tCustom packets added with this flag will override the existing packet.\n\tAlternate: --add-packet\n" +
                "-pf [file]: Adds a parameter file, where each line is an argument to pass into\n" +
                "\tSMProxy, as if done through the command line.\n\tAlternate: --parameter-file\n" +
                "-sp [packet]:[direction],...: Suppresses a packet.  [packet] is a packet ID,\n" +
                "\tand [direction] is any combination of 'C' and 'S', for which endpoint\n" + 
                "\tto deny the packets to.\n\tAlternate: --suppress-packet\n" +
                "-pv [version]: Manually set the protocol version number, in decimal. Default: 29\n\tAlternate: --protocol-version\n" +
                "-ep [value]: Changes the local endpoint. Default: 127.0.0.1\n\tAlternate: --endpoint\n" +
                "-sl: Suppress log.  Completely stops logging.\n\tAlternate: --suppress-log\n" +
                "-ps: Enable persistent sessions.  This will wait for the user to type \"quit\"\n" +
                "\tinto the console before closing.  When disabled, SMProxy will exit\n" +
                "\tafter a single session is complete.\n\tAlternate: --persistent-session\n");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "-?" || args[0] == "/?")
            {
                DisplayHelp();
                return;
            }

            for (int i = 0; i < args.Length - 2; i++)
            {
                if (!args[i].StartsWith("-"))
                {
                    DisplayHelp();
                    return;
                }
                switch (args[i])
                {
                    case "-o":
                    case "--output":
                        OutputFile = args[i + 1];
                        i++;
                        break;
                    case "-p":
                    case "--port":
                        LocalPort = int.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-f":
                    case "--filter":
                        FilterOutput = true;
                        foreach (string s in args[i + 1].Split(','))
                            Filter.Add(byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
                        i++;
                        break;
                    case "-!f":
                    case "--!filter":
                        IgnoreFilterOutput = true;
                        foreach (string s in args[i + 1].Split(','))
                            IgnoreFilter.Add(byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
                        i++;
                        break;
                    case "-sc":
                    case "--suppress-client":
                        SuppressClient = true;
                        break;
                    case "-ss":
                    case "--supress-server":
                        SuppressServer = true;
                        break;
                    case "-pr":
                    case "--enable-profiling":
                        EnableProfiling = true;
                        break;
                    case "-pv":
                    case "--protocol-version":
                        ProtocolVersion = int.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-ap":
                    case "--add-packet":
                        string[] parts = args[i + 1].Split(':');
                        byte id = byte.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
                        string direction = parts[2];
                        if (direction.ToUpper().Contains("C"))
                            CustomClientPackets.Add(id, parts[0] + ":" + parts[3]);
                        if (direction.ToUpper().Contains("S"))
                            CustomServerPackets.Add(id, parts[0] + ":" + parts[3]);
                        i++;
                        break;
                    case "-pf":
                    case "--parameter-file":
                        StreamReader sr = new StreamReader(args[i + 1]);
                        string file = sr.ReadToEnd();
                        sr.Close();

                        file = file.Replace("\r", "");
                        string[] lines = file.Split('\n');
                        foreach (string line in lines)
                        {
                            if (string.IsNullOrEmpty(line.Trim()))
                                continue;
                            if (line.Trim().StartsWith("#"))
                                continue;
                            args = args.Take(args.Length - 1).Concat(line.Split(' ')).Concat(new string[] { args[args.Length - 1] }).ToArray();
                        }
                        i++;
                        break;
                    case "-sp":
                    case "--suppress-packet":
                        parts = args[i + 1].Split(',');
                        foreach (string part in parts)
                        {
                            string[] subParts = part.Split(':');
                            if (subParts[1].Contains("C"))
                                ClientDenyPackets.Add(byte.Parse(subParts[0], System.Globalization.NumberStyles.HexNumber));
                            if (subParts[1].Contains("S"))
                                ServerDenyPackets.Add(byte.Parse(subParts[0], System.Globalization.NumberStyles.HexNumber));
                        }
                        i++;
                        break;
                    case "-sl":
                    case "--suppress-log":
                        SuppressLog = true;
                        break;
                    case "-ep":
                    case "--endpoint":
                        LocalEndpoint = args[i + 1];
                        i++;
                        break;
                    case "-ps":
                    case "--persistent-session":
                        PersistentSessions = true;
                        break;
                    default:
                        DisplayHelp();
                        return;
                }
            }

            ServerAddress = args[args.Length - 1];

            if (ServerAddress.Contains(":"))
            {
                RemotePort = int.Parse(ServerAddress.Split(':')[1]);
                ServerAddress = ServerAddress.Split(':')[0];
            }

            if (!SuppressLog)
            {
                outputLogger = new StreamWriter(OutputFile);

                outputLogger.WriteLine("SMProxy: Log opened at " + DateTime.Now.ToLongTimeString());
                outputLogger.WriteLine("Proxy parameters:");
                outputLogger.WriteLine("Local Endpoint: 127.0.0.1:" + LocalPort);
                outputLogger.WriteLine("Remote Endpoint: " + ServerAddress + ":" + RemotePort);
                if (FilterOutput)
                {
                    string values = "";
                    foreach (byte b in Filter)
                        values += "0x" + b.ToString("x") + ",";
                    outputLogger.WriteLine("Output filter: " + values.Remove(values.Length - 1));
                }
                if (IgnoreFilterOutput)
                {
                    string values = "";
                    foreach (byte b in IgnoreFilter)
                        values += "0x" + b.ToString("x") + ",";
                    outputLogger.WriteLine("Ignored filter: " + values.Remove(values.Length - 1));
                }
                if (CustomClientPackets.Count != 0)
                {
                    outputLogger.WriteLine("Custom Client->Server packets:");
                    foreach (var kvp in CustomClientPackets)
                        outputLogger.WriteLine("0x" + kvp.Key.ToString("x") + ":" + kvp.Value);
                }
                if (CustomServerPackets.Count != 0)
                {
                    outputLogger.WriteLine("Custom Server->Client packets:");
                    foreach (var kvp in CustomServerPackets)
                        outputLogger.WriteLine("0x" + kvp.Key.ToString("x") + ":" + kvp.Value);
                }
                if (ServerDenyPackets.Count != 0)
                {
                    outputLogger.Write("Suppressing packets from server: ");
                    foreach (var packet in ServerDenyPackets)
                        outputLogger.Write(packet.ToString("x") + ",");
                    outputLogger.WriteLine();
                }
                if (ClientDenyPackets.Count != 0)
                {
                    outputLogger.Write("Suppressing packets from client: ");
                    foreach (var packet in ClientDenyPackets)
                        outputLogger.Write(packet.ToString("x") + ",");
                    outputLogger.WriteLine();
                }
                if (SuppressClient)
                    outputLogger.WriteLine("Suppressing client->server output");
                if (SuppressServer)
                    outputLogger.WriteLine("Suppressing server->client output");
                outputLogger.WriteLine();
            }

            Listener = new TcpListener(IPAddress.Parse(LocalEndpoint), LocalPort);
            Listener.Start();

            Console.WriteLine("Listening on " + LocalEndpoint + ":" + LocalPort);

            if (PersistentSessions)
            {
                Listener.BeginAcceptTcpClient(AcceptAsync, null);

                while (Console.ReadLine() != "quit") ;
            }
            else
            {
                TcpClient client = Listener.AcceptTcpClient();
                TcpClient server = new TcpClient(ServerAddress, RemotePort);

                HandleConnection(outputLogger, client, server);
            }

            outputLogger.Close();
        }

        static void AcceptAsync(IAsyncResult Result)
        {
            TcpClient client = Listener.EndAcceptTcpClient(Result);
            Listener.BeginAcceptTcpClient(AcceptAsync, null);
            try
            {
                TcpClient server = new TcpClient(ServerAddress, RemotePort);

                HandleConnection(outputLogger, client, server);
            }
            catch
            {
                Console.WriteLine("Error connecting to server.");
                try
                {
                    byte[] errorMessage = new byte[] { 0xFF };
                    errorMessage = errorMessage.Concat(PacketReader.MakeString("[Proxy]: Unable to connect to server")).ToArray();
                    client.GetStream().Write(errorMessage, 0, errorMessage.Length);
                }
                catch { }
            }
        }
    }
}
