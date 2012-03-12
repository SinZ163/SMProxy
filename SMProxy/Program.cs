using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibMinecraft.Model;

namespace SMProxy
{
    static class Program
    {
        static TcpListener Listener;
        static bool ClientDirty = false, ServerDirty = false;
        static bool SuppressServer = false, SuppressClient = false, FilterOutput = false;
        static string OutputFile = "output.txt", ServerAddress = null;
        static List<byte> Filter = new List<byte>();
        static int LocalPort = 25564, RemotePort = 25565;

        static void DisplayHelp()
        {
            Console.WriteLine("Usage: SMProxy.exe [flags] [server address]\n" +
                "Flags:\n" +
                "-o [file]: Set output to [file].  Default: output.txt\n" +
                "-p [port]: Set local endpoint to listen on [port].  Default: 25564\n" +
                "-f [filter]: Logs only packets that match [filter].\n" +
                "\t[filter] is a comma-delimited list of packet IDs, in hex.\n" +
                "\tExample: -f 01,02 would restrict output to handshake and login packets.\n" +
                "-sc: Suppress client.  Suppresses logging for client->server packets.\n" +
                "-ss: Suppress server.  Suppresses logging for server->client packets.");
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
                        OutputFile = args[i + 1];
                        i++;
                        break;
                    case "-p":
                        LocalPort = int.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-f":
                        FilterOutput = true;
                        foreach (string s in args[i + 1].Split(','))
                            Filter.Add(byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
                        i++;
                        break;
                    case "-sc":
                        SuppressClient = true;
                        break;
                    case "-ss":
                        SuppressServer = true;
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

            StreamWriter outputLogger;
            outputLogger = new StreamWriter(OutputFile);

            outputLogger.WriteLine("SMProxy: Log opened at " + DateTime.Now.ToLongTimeString());
            outputLogger.WriteLine("Proxy parameters:");
            outputLogger.WriteLine("Local Endpoint: 127.0.0.1" + LocalPort);
            outputLogger.WriteLine("Remote Endpoint: " + ServerAddress + ":" + RemotePort);
            if (FilterOutput)
            {
                string values = "";
                foreach (byte b in Filter)
                    values += "0x" + b.ToString("x") + ",";
                outputLogger.WriteLine("Output filter: " + values.Remove(values.Length - 1));
            }
            if (SuppressClient)
                outputLogger.WriteLine("Suppressing client->server output");
            if (SuppressServer)
                outputLogger.WriteLine("Suppressing server->client output");
            outputLogger.WriteLine();

            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), LocalPort); // TODO: Allow for alternative endpoint to be specified
            Listener.Start();

            Console.WriteLine("Listening on 127.0.0.1:" + LocalPort);

            TcpClient client = Listener.AcceptTcpClient();
            TcpClient server = new TcpClient(ServerAddress, RemotePort);

            Console.WriteLine("Connected to remote server.");

            while (client.Connected && server.Connected)
            {
                if (client.Available != 0)
                {
                    int data = client.GetStream().ReadByte();
                    if (data == -1)
                        break;

                    if (ClientDirty)
                    {
                        try
                        {
                            server.GetStream().WriteByte((byte)data);
                            outputLogger.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [RAW CLIENT->SERVER]: " + ((byte)data).ToString("x"));
                        }
                        catch { }
                    }
                    else
                    {
                        PacketReader pr = new PacketReader(client);
                        try
                        {
                            // Parse packet
                            switch (data)
                            {
                                case 0x00: // Keep-alive
                                    LogPacket(outputLogger, true, 0x00, pr,
                                        "Keep-Alive", pr.ReadInt());
                                    break;
                                case 0x01: // Login Request
                                    LogPacket(outputLogger, true, 0x01, pr,
                                        "Protocol Version", pr.ReadInt(),
                                        "Username", pr.ReadString());
                                    pr.ReadString();
                                    pr.Read(11);
                                    break;
                                case 0x02: // Handshake
                                    LogPacket(outputLogger, true, 0x02, pr,
                                        "Username/Hostname", pr.ReadString());
                                    break;
                                case 0x03: // Chat Message
                                    string msg = pr.ReadString();
                                    LogPacket(outputLogger, true, 0x03, pr,
                                        "Text", msg);
                                    break;
                                case 0x07: // Use Entity
                                    LogPacket(outputLogger, true, 0x07, pr,
                                        "User Entity ID", pr.ReadInt(),
                                        "Target Entity ID", pr.ReadInt(),
                                        "Left-Click", pr.ReadBoolean());
                                    break;
                                case 0x09: // Respawn
                                    LogPacket(outputLogger, true, 0x09, pr,
                                        "Dimension", pr.ReadInt(),
                                        "Difficulty", pr.ReadByte(),
                                        "Creative", pr.ReadBoolean(),
                                        "World Height", pr.ReadShort(),
                                        "Level Type", pr.ReadString());
                                    break;
                                case 0x0A: // Player
                                    LogPacket(outputLogger, true, 0x0A, pr,
                                        "On Ground", pr.ReadBoolean());
                                    break;
                                case 0x0B: // Player Position
                                    LogPacket(outputLogger, true, 0x0B, pr,
                                        "X", pr.ReadDouble(),
                                        "Y", pr.ReadDouble(),
                                        "Stance", pr.ReadDouble(),
                                        "Z", pr.ReadDouble(),
                                        "On Ground", pr.ReadBoolean());
                                    break;
                                case 0x0C: // Player Look
                                    LogPacket(outputLogger, true, 0x0C, pr,
                                        "Yaw", pr.ReadFloat(),
                                        "Pitch", pr.ReadFloat(),
                                        "On Ground", pr.ReadBoolean());
                                    break;
                                case 0x0D: // Player Position & Look
                                    LogPacket(outputLogger, true, 0x0D, pr,
                                        "X", pr.ReadDouble(),
                                        "Y", pr.ReadDouble(),
                                        "Stance", pr.ReadDouble(),
                                        "Z", pr.ReadDouble(),
                                        "Yaw", pr.ReadFloat(),
                                        "Pitch", pr.ReadFloat(),
                                        "On Ground", pr.ReadBoolean());
                                    break;
                                case 0x0E: // Player Digging
                                    LogPacket(outputLogger, true, 0x0E, pr,
                                        "Status", pr.ReadByte(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadByte(),
                                        "Z", pr.ReadInt(),
                                        "Face", pr.ReadByte());
                                    break;
                                case 0x0F: // Player Block Placement
                                    LogPacket(outputLogger, true, 0x0F, pr,
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadByte(),
                                        "Z", pr.ReadInt(),
                                        "Direction", pr.ReadByte(),
                                        "Held Item", pr.ReadSlot());
                                    break;
                                case 0x10: // Held Item Change
                                    LogPacket(outputLogger, true, 0x10, pr,
                                        "Slot ID", pr.ReadShort());
                                    break;
                                case 0x12: // Animation
                                    LogPacket(outputLogger, true, 0x12, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Animation ID", pr.ReadByte());
                                    break;
                                case 0x13: // Entity Action
                                    LogPacket(outputLogger, true, 0x13, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Action ID", pr.ReadByte());
                                    break;
                                case 0x65: // Close Window
                                    LogPacket(outputLogger, true, 0x65, pr,
                                        "Window ID", pr.ReadByte());
                                    break;
                                case 0x66: // Click Window
                                    LogPacket(outputLogger, true, 0x66, pr,
                                        "Window ID", pr.ReadByte(),
                                        "Slot ID", pr.ReadShort(),
                                        "Right Click", pr.ReadBoolean(),
                                        "Action Number", pr.ReadShort(),
                                        "Shift", pr.ReadBoolean(),
                                        "Clicked Item", pr.ReadSlot());
                                    break;
                                case 0x6A: // Confirm Transaction
                                    LogPacket(outputLogger, true, 0x19, pr,
                                        "Window ID", pr.ReadByte(),
                                        "Action Number", pr.ReadShort(),
                                        "Accepted", pr.ReadBoolean());
                                    break;
                                case 0x6B: // Creative Inventory Action
                                    LogPacket(outputLogger, true, 0x6B, pr,
                                        "Slot ID", pr.ReadShort(),
                                        "Clicked Item", pr.ReadSlot());
                                    break;
                                case 0x6C: // Enchant Item
                                    LogPacket(outputLogger, true, 0x6C, pr,
                                        "Window ID", pr.ReadByte(),
                                        "Enchantment", pr.ReadByte());
                                    break;
                                case 0x82: // Update Sign
                                    LogPacket(outputLogger, true, 0x82, pr,
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadShort(),
                                        "Z", pr.ReadInt(),
                                        "Text1", pr.ReadString(),
                                        "Text2", pr.ReadString(),
                                        "Text3", pr.ReadString(),
                                        "Text4", pr.ReadString());
                                    break;
                                case 0xFA: // Plugin Message
                                    string s = pr.ReadString();
                                    short l = pr.ReadShort();
                                    LogPacket(outputLogger, true, 0xFA, pr,
                                        "Channel", s,
                                        "Length", l,
                                        "Data", pr.Read(l));
                                    break;
                                case 0xFE: // Server List Ping
                                    LogPacket(outputLogger, true, 0xFE, pr);
                                    break;
                                case 0xFF: // Disconnect
                                    LogPacket(outputLogger, true, 0xFF, pr,
                                        "Reason", pr.ReadString());
                                    server.GetStream().WriteByte((byte)data);
                                    server.GetStream().Write(pr.Payload, 0, pr.Payload.Length);
                                    client.Close();
                                    server.Close();
                                    break;
                                default:
                                    ClientDirty = true;
                                    Console.WriteLine("WARNING: Client send unrecognized packet!  Switching to raw log mode.");
                                    outputLogger.WriteLine("WARNING: Client sent unrecognized packet!  Switching to raw log mode.");
                                    break;
                            }
                        }
                        catch
                        {
                            ClientDirty = true;
                            Console.WriteLine("WARNING: Client sent unrecognized packet!  Switching to raw log mode.");
                            outputLogger.WriteLine("WARNING: Client sent unrecognized packet!  Switching to raw log mode.");
                        }
                        finally
                        {
                            if (server.Connected)
                            {
                                server.GetStream().WriteByte((byte)data);
                                server.GetStream().Write(pr.Payload, 0, pr.Payload.Length);
                            }
                        }
                    }
                }
                if (server.Connected && client.Connected && server.Available != 0)
                {
                    int data = server.GetStream().ReadByte();
                    if (data == -1)
                        break;

                    if (ServerDirty)
                    {
                        try
                        {
                            client.GetStream().WriteByte((byte)data);
                            outputLogger.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [RAW SERVER->CLIENT]: " + ((byte)data).ToString("x"));
                        }
                        catch { }
                    }
                    else
                    {
                        PacketReader pr = new PacketReader(server);
                        try
                        {
                            switch (data)
                            {
                                case 0x00: // Keep-alive
                                    LogPacket(outputLogger, false, 0x00, pr,
                                        "Keep-Alive", pr.ReadInt());
                                    break;
                                case 0x01: // Login Request
                                    LogPacket(outputLogger, false, 0x01, pr,
                                        "Protocol Version", pr.ReadInt(),
                                        "[unused]", pr.ReadString(),
                                        "Level Type", pr.ReadString(),
                                        "Server Mode", pr.ReadInt(),
                                        "Dimension", pr.ReadInt(),
                                        "Difficulty", pr.ReadByte(),
                                        "World Height", pr.ReadByte(),
                                        "Max Players", pr.ReadByte());
                                    break;
                                case 0x02: // Handshake
                                    LogPacket(outputLogger, false, 0x02, pr,
                                        "Server Hash", pr.ReadString());
                                    break;
                                case 0x03: // Chat Message
                                    string msg = pr.ReadString();
                                    Console.WriteLine("Chat: " + msg);
                                    LogPacket(outputLogger, false, 0x03, pr,
                                        "Text", msg);
                                    break;
                                case 0x04: // Time Update
                                    LogPacket(outputLogger, false, 0x04, pr,
                                        "Time", pr.ReadLong());
                                    break;
                                case 0x05: // Entity Equipment
                                    LogPacket(outputLogger, false, 0x05, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Slot", pr.ReadShort(),
                                        "Item ID", pr.ReadShort(),
                                        "Damage", pr.ReadShort());
                                    break;
                                case 0x06: // Spawn Position
                                    LogPacket(outputLogger, false, 0x06, pr,
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt());
                                    break;
                                case 0x08: // Health Update
                                    LogPacket(outputLogger, false, 0x08, pr,
                                        "Health", pr.ReadShort(),
                                        "Food", pr.ReadShort(),
                                        "Food Saturation", pr.ReadFloat());
                                    break;
                                case 0x09: // Respawn
                                    LogPacket(outputLogger, false, 0x09, pr,
                                        "Dimension", pr.ReadInt(),
                                        "Difficulty", pr.ReadByte(),
                                        "Creative", pr.ReadBoolean(),
                                        "World Height", pr.ReadShort(),
                                        "Level Type", pr.ReadString());
                                    break;
                                case 0x0D: // Player Position & Look
                                    LogPacket(outputLogger, false, 0x0D, pr,
                                        "X", pr.ReadDouble(),
                                        "Stance", pr.ReadDouble(),
                                        "Y", pr.ReadDouble(),
                                        "Z", pr.ReadDouble(),
                                        "Yaw", pr.ReadFloat(),
                                        "Pitch", pr.ReadFloat(),
                                        "On Ground", pr.ReadBoolean());
                                    break;
                                case 0x0E: // Player Digging
                                    LogPacket(outputLogger, false, 0x0E, pr,
                                        "Status", pr.ReadByte(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadByte(),
                                        "Z", pr.ReadInt(),
                                        "Face", pr.ReadByte());
                                    break;
                                case 0x12: // Animation
                                    LogPacket(outputLogger, false, 0x12, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Animation ID", pr.ReadByte());
                                    break;
                                case 0x14: // Spawn Named Entity
                                    LogPacket(outputLogger, false, 0x14, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Name", pr.ReadString(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt(),
                                        "Yaw", pr.ReadByte(),
                                        "Pitch", pr.ReadByte(),
                                        "Current Item", pr.ReadShort());
                                    break;
                                case 0x15: // Spawn Dropped Item
                                    LogPacket(outputLogger, false, 0x15, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Item ID", pr.ReadShort(),
                                        "Count", pr.ReadByte(),
                                        "Metadata", pr.ReadShort(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt(),
                                        "Yaw", pr.ReadByte(),
                                        "Pitch", pr.ReadByte(),
                                        "Roll", pr.ReadByte());
                                    break;
                                case 0x16: // Collect Item
                                    LogPacket(outputLogger, false, 0x16, pr,
                                        "Collected Item ID", pr.ReadInt(),
                                        "Collecting Player ID", pr.ReadInt());
                                    break;
                                case 0x17: // Spawn Object/Vehicle
                                    int EID = pr.ReadInt();
                                    byte type = pr.ReadByte();
                                    int X = pr.ReadInt();
                                    int Y = pr.ReadInt();
                                    int Z = pr.ReadInt();
                                    int fireballID = pr.ReadInt();
                                    if (fireballID == 0)
                                    {
                                        LogPacket(outputLogger, false, 0x17, pr,
                                            "Entity ID", EID,
                                            "Type", type,
                                            "X", X,
                                            "Y", Y,
                                            "Z", Z,
                                            "Fireball Thrower ID", fireballID);
                                    }
                                    else
                                    {
                                        LogPacket(outputLogger, false, 0x17, pr,
                                            "Entity ID", EID,
                                            "Type", type,
                                            "X", X,
                                            "Y", Y,
                                            "Z", Z,
                                            "Fireball Thrower ID", fireballID,
                                            "Speed X", pr.ReadShort(),
                                            "Speed Y", pr.ReadShort(),
                                            "Speed Z", pr.ReadShort());
                                    }
                                    break;
                                case 0x18: // Spawn Mob
                                    LogPacket(outputLogger, false, 0x18, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Type", pr.ReadByte(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt(),
                                        "Yaw", pr.ReadByte(),
                                        "Pitch", pr.ReadByte(),
                                        "Head Yaw", pr.ReadByte(),
                                        "Metadata", pr.ReadMobMetadata());
                                    break;
                                case 0x19: // Spawn Painting
                                    LogPacket(outputLogger, false, 0x19, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Title", pr.ReadString(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt(),
                                        "Direction", pr.ReadInt());
                                    break;
                                case 0x1A: // Spawn Exp Orb
                                    LogPacket(outputLogger, false, 0x1A, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt(),
                                        "Count", pr.ReadShort());
                                    break;
                                case 0x1C: // Entity Velocity
                                    LogPacket(outputLogger, false, 0x1C, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Velocity X", pr.ReadShort(),
                                        "Velocity Y", pr.ReadShort(),
                                        "Velocity Z", pr.ReadShort());
                                    break;
                                case 0x1D: // Destroy Entity
                                    LogPacket(outputLogger, false, 0x1D, pr,
                                        "Entity ID", pr.ReadInt());
                                    break;
                                case 0x1E: // Entity
                                    LogPacket(outputLogger, false, 0x1E, pr,
                                        "Entity ID", pr.ReadInt());
                                    break;
                                case 0x1F: // Entity Relative Move
                                    LogPacket(outputLogger, false, 0x1F, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Delta X", pr.ReadByte(),
                                        "Delta Y", pr.ReadByte(),
                                        "Delta Z", pr.ReadByte());
                                    break;
                                case 0x20: // Entity Look
                                    LogPacket(outputLogger, false, 0x20, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Yaw", pr.ReadByte(),
                                        "Pitch", pr.ReadByte());
                                    break;
                                case 0x21: // Entity Look & Relative Move
                                    LogPacket(outputLogger, false, 0x21, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Delta X", pr.ReadByte(),
                                        "Delta Y", pr.ReadByte(),
                                        "Delta Z", pr.ReadByte(),
                                        "Yaw", pr.ReadByte(),
                                        "Pitch", pr.ReadByte());
                                    break;
                                case 0x22: // Entity Teleport
                                    LogPacket(outputLogger, false, 0x22, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt(),
                                        "Yaw", pr.ReadByte(),
                                        "Pitch", pr.ReadByte());
                                    break;
                                case 0x23: // Entity Head Look
                                    LogPacket(outputLogger, false, 0x23, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Head Yaw", pr.ReadByte());
                                    break;
                                case 0x26: // Entity Status
                                    LogPacket(outputLogger, false, 0x26, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Entity Status", pr.ReadByte());
                                    break;
                                case 0x27: // Attach Entity
                                    LogPacket(outputLogger, false, 0x27, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Vehicle ID", pr.ReadInt());
                                    break;
                                case 0x28: // Entity Metadata
                                    LogPacket(outputLogger, false, 0x28, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Entity Metadata", pr.ReadMobMetadata());
                                    break;
                                case 0x29: // Entity Effect
                                    LogPacket(outputLogger, false, 0x29, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Effect ID", pr.ReadByte(),
                                        "Amplifier", pr.ReadByte(),
                                        "Duration", pr.ReadShort());
                                    break;
                                case 0x2A: // Remove Entity Effect
                                    LogPacket(outputLogger, false, 0x2A, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "Effect ID", pr.ReadByte());
                                    break;
                                case 0x2B: // Set Exp
                                    LogPacket(outputLogger, false, 0x2B, pr,
                                        "Experience Bar", pr.ReadFloat(),
                                        "Level", pr.ReadShort(),
                                        "Total", pr.ReadShort());
                                    break;
                                case 0x32: // Map Column Allocation
                                    LogPacket(outputLogger, false, 0x32, pr,
                                        "X", pr.ReadInt(),
                                        "Z", pr.ReadInt(),
                                        "Allocate", pr.ReadBoolean());
                                    break;
                                case 0x33: // Map Chunks
                                    int mapX = pr.ReadInt();
                                    int mapZ = pr.ReadInt();
                                    bool groundUp = pr.ReadBoolean();
                                    ushort primaryBitMap = (ushort)pr.ReadShort();
                                    ushort addBitMap = (ushort)pr.ReadShort();
                                    int compressedSize = pr.ReadInt();
                                    LogPacket(outputLogger, false, 0x33, pr,
                                        "X", mapX,
                                        "Z", mapZ,
                                        "Ground-Up Continuous", groundUp,
                                        "Primary Bit Map", primaryBitMap,
                                        "Add Bit Map", addBitMap,
                                        "Compressed Size", compressedSize,
                                        "[unused]", pr.ReadInt(),
                                        "Data", pr.Read(compressedSize));
                                    break;
                                case 0x34: // Multi-Block Change
                                    int cX = pr.ReadInt();
                                    int cZ = pr.ReadInt();
                                    short recordCount = pr.ReadShort();
                                    int size = pr.ReadInt();
                                    LogPacket(outputLogger, false, 0x34, pr,
                                        "Chunk X", cX,
                                        "Chunk Z", cZ,
                                        "Blocks Affected", recordCount,
                                        "Data Size", size,
                                        "Data", pr.Read(size));
                                    break;
                                case 0x35: // Block Change
                                    LogPacket(outputLogger, false, 0x35, pr,
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadByte(),
                                        "Z", pr.ReadInt(),
                                        "ID", pr.ReadByte(),
                                        "Metadata", pr.ReadByte());
                                    break;
                                case 0x36: // Block Action
                                    LogPacket(outputLogger, false, 0x36, pr,
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadShort(),
                                        "Z", pr.ReadInt(),
                                        "Data[0]", pr.ReadByte(),
                                        "Data[1]", pr.ReadByte());
                                    break;
                                case 0x3C: // Explosion
                                    double eX = pr.ReadDouble();
                                    double eY = pr.ReadDouble();
                                    double eZ = pr.ReadDouble();
                                    float unknown = pr.ReadFloat();
                                    int blocksAffected = pr.ReadInt();
                                    LogPacket(outputLogger, false, 0x3C, pr,
                                        "X", eX,
                                        "Y", eY,
                                        "Z", eZ,
                                        "[unknown]", unknown,
                                        "Blocks Affected", blocksAffected,
                                        "Data", pr.Read(blocksAffected * 3));
                                    break;
                                case 0x3D: // Sound/Particle Effect
                                    LogPacket(outputLogger, false, 0x3D, pr,
                                        "Effect ID", pr.ReadInt(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadByte(),
                                        "Z", pr.ReadInt(),
                                        "Data", pr.ReadInt());
                                    break;
                                case 0x46: // Change Game State
                                    LogPacket(outputLogger, false, 0x46, pr,
                                        "Reason", pr.ReadByte(),
                                        "Game Mode", pr.ReadByte());
                                    break;
                                case 0x47: // Thunderbolt
                                    LogPacket(outputLogger, false, 0x47, pr,
                                        "Entity ID", pr.ReadInt(),
                                        "[unknown]", pr.ReadBoolean(),
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadInt(),
                                        "Z", pr.ReadInt());
                                    break;
                                case 0x64: // Open Window
                                    LogPacket(outputLogger, false, 0x64, pr,
                                        "Window ID", pr.ReadByte(),
                                        "Inventory Type", pr.ReadByte(),
                                        "Window Title", pr.ReadString(),
                                        "Slot Count", pr.ReadByte());
                                    break;
                                case 0x65: // Close Window
                                    LogPacket(outputLogger, false, 0x65, pr,
                                        "Window ID", pr.ReadByte());
                                    break;
                                case 0x67: // Set Slot
                                    LogPacket(outputLogger, false, 0x67, pr,
                                        "Window ID", pr.ReadByte(),
                                        "Slot Index", pr.ReadShort(),
                                        "Slot", pr.ReadSlot());
                                    break;
                                case 0x68: // Set Window Items
                                    // This one is tricky, and done in a custom fashion
                                    byte windowID = pr.ReadByte();
                                    short count = pr.ReadShort();
                                    string dump = "[";
                                    for (int i = 0; i < count; i++)
                                        dump += pr.ReadSlot().ToString() + ", ";
                                    dump = dump.Remove(dump.Length - 1) + "]";
                                    LogPacket(outputLogger, false, 0x68, pr,
                                        "Window ID", windowID,
                                        "Count", count,
                                        "Slots", dump);
                                    break;
                                case 0x69: // Update Window Property
                                    LogPacket(outputLogger, false, 0x69, pr,
                                        "Window ID", pr.ReadByte(),
                                        "Property", pr.ReadShort(),
                                        "Value", pr.ReadShort());
                                    break;
                                case 0x6A: // Confirm Transaction
                                    LogPacket(outputLogger, false, 0x6A, pr,
                                        "Window ID", pr.ReadByte(),
                                        "Action Number", pr.ReadShort(),
                                        "Accepted", pr.ReadBoolean());
                                    break;
                                case 0x82: // Update Sign
                                    LogPacket(outputLogger, false, 0x82, pr,
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadShort(),
                                        "Z", pr.ReadInt(),
                                        "Text1", pr.ReadString(),
                                        "Text2", pr.ReadString(),
                                        "Text3", pr.ReadString(),
                                        "Text4", pr.ReadString());
                                    break;
                                case 0x83: // Item Data
                                    short itemType = pr.ReadShort();
                                    short itemID = pr.ReadShort();
                                    byte length = pr.ReadByte();
                                    LogPacket(outputLogger, false, 0x83, pr,
                                        "Item Type", itemType,
                                        "Item ID", itemID,
                                        "Length", length,
                                        "Data", pr.Read(length));
                                    break;
                                case 0x84: // Update Tile Entity
                                    LogPacket(outputLogger, false, 0x84, pr,
                                        "X", pr.ReadInt(),
                                        "Y", pr.ReadShort(),
                                        "Z", pr.ReadInt(),
                                        "Action", pr.ReadByte(),
                                        "Data[0]", pr.ReadInt(),
                                        "Data[1]", pr.ReadInt(),
                                        "Data[2]", pr.ReadInt());
                                    break;
                                case 0xC8: // Update Statistic
                                    LogPacket(outputLogger, false, 0xC8, pr,
                                        "Statistic ID", pr.ReadInt(),
                                        "Amount", pr.ReadByte());
                                    break;
                                case 0xC9: // Player List Item
                                    LogPacket(outputLogger, false, 0xC9, pr,
                                        "Player Name", pr.ReadString(),
                                        "Online", pr.ReadBoolean(),
                                        "Ping", pr.ReadShort());
                                    break;
                                case 0xFA: // Plugin Message
                                    string s = pr.ReadString();
                                    short l = pr.ReadShort();
                                    LogPacket(outputLogger, false, 0xFA, pr,
                                        "Channel", s,
                                        "Length", l,
                                        "Data", pr.Read(l));
                                    break;
                                case 0xFE: // Server List Ping
                                    LogPacket(outputLogger, false, 0xFE, pr);
                                    break;
                                case 0xFF: // Disconnect
                                    LogPacket(outputLogger, false, 0xFF, pr,
                                        "Reason", pr.ReadString());
                                    server.GetStream().WriteByte((byte)data);
                                    server.GetStream().Write(pr.Payload, 0, pr.Payload.Length);
                                    client.Close();
                                    server.Close();
                                    break;
                                default:
                                    ServerDirty = true;
                                    Console.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                    outputLogger.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                    break;
                            }
                        }
                        catch
                        {
                            ServerDirty = true;
                            Console.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                            outputLogger.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                        }
                        finally
                        {
                            if (client.Connected)
                            {
                                client.GetStream().WriteByte((byte)data);
                                client.GetStream().Write(pr.Payload, 0, pr.Payload.Length);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Disconnected.");

            outputLogger.Close();
        }

        static void LogPacket(StreamWriter sw, bool ClientToServer, byte PacketID, PacketReader pr, params object[] args)
        {
            if (FilterOutput && !Filter.Contains(PacketID))
                return;
            if (ClientToServer && SuppressClient)
                return;
            if (!ClientToServer && SuppressServer)
                return;
            if (ClientToServer)
                sw.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [CLIENT->SERVER]: " +
                    ((LibMinecraft.Model.PacketID)PacketID).ToString() + " (0x" + PacketID.ToString("x") + ")");
            else
                sw.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [SERVER->CLIENT]: " +
                    ((LibMinecraft.Model.PacketID)PacketID).ToString() + " (0x" + PacketID.ToString("x") + ")");
            sw.WriteLine("\t[" + DumpArray(pr.Payload) + "]");
            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i + 1] is byte[])
                    sw.WriteLine("\t" + args[i].ToString() + " (Byte[]): [" + DumpArray((byte[])args[i + 1]) + "]");
                else
                    sw.WriteLine("\t" + args[i].ToString() + " (" + args[i + 1].GetType().Name + "): " + args[i + 1]);
            }
            sw.Flush();
        }

        static string DumpArray(byte[] resp)
        {
            string res = "";
            foreach (byte b in resp)
                res += b.ToString("x2") + ":";
            return res.Remove(res.Length - 1);
        }

        public static long CopyTo(this Stream source, Stream destination)
        {
            byte[] buffer = new byte[2048];
            int bytesRead;
            long totalBytes = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
            }
            return totalBytes;
        }
    }

    class PacketReader
    {
        public byte[] Payload { get; set; }
        public TcpClient Client { get; set; }
        private Stream s { get; set; }

        public PacketReader(TcpClient Client)
        {
            Payload = new byte[0];
            this.Client = Client;
            this.s = Client.GetStream();
        }

        public PacketReader(Stream s)
        {
            Payload = new byte[0];
            this.s = s;
        }

        /// <summary>
        /// Strings the length.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected int StringLength(string str)
        {
            return 2 + str.Length * 2;
        }

        /// <summary>
        /// Makes the string.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeString(String msg)
        {
            short len = IPAddress.HostToNetworkOrder((short)msg.Length);
            byte[] a = BitConverter.GetBytes(len);
            byte[] b = Encoding.BigEndianUnicode.GetBytes(msg);
            return a.Concat(b).ToArray();
        }

        /// <summary>
        /// Makes the int.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeInt(int i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        /// <summary>
        /// Makes the absolute int.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeAbsoluteInt(double i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(i * 32.0)));
        }

        /// <summary>
        /// Makes the long.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeLong(long i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        /// <summary>
        /// Makes the short.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeShort(short i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        public byte[] MakeUShort(ushort i)
        {
            return MakeShort((short)i);
        }

        /// <summary>
        /// Makes the double.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeDouble(double d)
        {
            byte[] b = BitConverter.GetBytes(d);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return b;
        }

        /// <summary>
        /// Makes the float.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeFloat(float f)
        {
            byte[] b = BitConverter.GetBytes(f);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return b;
        }

        /// <summary>
        /// Makes the packed byte.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte MakePackedByte(float f)
        {
            return (byte)(((Math.Floor(f) % 360) / 360) * 256);
        }

        /// <summary>
        /// 
        /// </summary>
        static byte[] BooleanArray = new byte[] { 0 };
        /// <summary>
        /// Makes the boolean.
        /// </summary>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeBoolean(Boolean b)
        {
            BooleanArray[0] = (byte)(b ? 1 : 0);
            return BooleanArray;
        }

        public Slot ReadSlot()
        {
            Slot slot = Slot.ReadSlot(s);
            Payload = Payload.Concat(slot.Data).ToArray();
            return slot;
        }

        public byte ReadByte()
        {
            byte b = (byte)s.ReadByte();
            Payload = Payload.Concat(new byte[] { b }).ToArray();
            return b;
        }

        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public int ReadInt()
        {
            return IPAddress.HostToNetworkOrder((int)Read(4));
        }

        /// <summary>
        /// Reads the short.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public short ReadShort()
        {
            return IPAddress.HostToNetworkOrder((short)Read(2));
        }

        /// <summary>
        /// Reads the long.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public long ReadLong()
        {
            return IPAddress.HostToNetworkOrder((long)Read(8));
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public double ReadDouble()
        {
            byte[] doubleArray = new byte[sizeof(double)];
            s.Read(doubleArray, 0, sizeof(double));
            Payload = Payload.Concat(doubleArray).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(doubleArray);
            return BitConverter.ToDouble(doubleArray, 0);
        }

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public unsafe float ReadFloat()
        {
            byte[] floatArray = new byte[sizeof(int)];
            s.Read(floatArray, 0, sizeof(int));
            Payload = Payload.Concat(floatArray).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(floatArray);
            int i = BitConverter.ToInt32(floatArray, 0);
            return *(float*)&i;
        }

        /// <summary>
        /// Reads the boolean.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Boolean ReadBoolean()
        {
            return ReadByte() == 1;
        }

        /// <summary>
        /// Reads the bytes.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] ReadBytes(int count)
        {
            byte[] b = new BinaryReader(s).ReadBytes(count);
            Payload = Payload.Concat(b).ToArray();
            return b;
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public String ReadString()
        {
            short len;
            byte[] a = new byte[2];
            a[0] = (byte)s.ReadByte();
            a[1] = (byte)s.ReadByte();
            len = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(a, 0));
            byte[] b = new byte[len * 2];
            for (int i = 0; i < len * 2; i++)
            {
                b[i] = (byte)s.ReadByte();
            }
            Payload = Payload.Concat(a.Concat(b)).ToArray();
            return Encoding.BigEndianUnicode.GetString(b);
        }

        public byte[] ReadMobMetadata()
        {
            byte[] b = new byte[0];
            byte value = 0;
            while (value != 127)
            {
                value = ReadByte();
                b = b.Concat(new byte[] { value }).ToArray();
                if (value != 127)
                {
                    switch (value >> 5)
                    {
                        case 0:
                            b = b.Concat(ReadBytes(1)).ToArray();
                            break;
                        case 1:
                            b = b.Concat(ReadBytes(2)).ToArray();
                            break;
                        case 2:
                        case 3:
                            b = b.Concat(ReadBytes(4)).ToArray();
                            break;
                        case 4:
                            b = b.Concat(ReadBytes(16)).ToArray();
                            break;
                        case 5:
                            b = b.Concat(ReadBytes(5)).ToArray();
                            break;
                        case 6:
                            b = b.Concat(ReadBytes(12)).ToArray();
                            break;
                    }
                }
            }
            return b;
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="msg">The MSG.</param>
        /// <remarks></remarks>
        public void WriteString(String msg)
        {

            short len = IPAddress.HostToNetworkOrder((short)msg.Length);
            byte[] a = BitConverter.GetBytes(len);
            byte[] b = Encoding.BigEndianUnicode.GetBytes(msg);
            byte[] c = a.Concat(b).ToArray();
            s.Write(c, 0, c.Length);
        }

        /// <summary>
        /// Writes the int.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="i">The i.</param>
        /// <remarks></remarks>
        public void WriteInt(int i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            s.Write(a, 0, a.Length);
        }

        /// <summary>
        /// Writes the long.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="i">The i.</param>
        /// <remarks></remarks>
        public void WriteLong(long i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            s.Write(a, 0, a.Length);
        }

        /// <summary>
        /// Writes the short.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="i">The i.</param>
        /// <remarks></remarks>
        public void WriteShort(short i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            s.Write(a, 0, a.Length);
        }

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="d">The d.</param>
        /// <remarks></remarks>
        public void WriteDouble(double d)
        {
            byte[] doubleArray = BitConverter.GetBytes(d);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(doubleArray);
            s.Write(doubleArray, 0, sizeof(double));
        }

        /// <summary>
        /// Writes the float.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="f">The f.</param>
        /// <remarks></remarks>
        public void WriteFloat(float f)
        {
            byte[] floatArray = BitConverter.GetBytes(f);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(floatArray);
            s.Write(floatArray, 0, sizeof(float));
        }

        /// <summary>
        /// Writes the boolean.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <remarks></remarks>
        public void WriteBoolean(Boolean b)
        {
            new BinaryWriter(s).Write(b);
        }

        /// <summary>
        /// Writes the bytes.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="b">The b.</param>
        /// <remarks></remarks>
        public void WriteBytes(byte[] b)
        {
            new BinaryWriter(s).Write(b);
        }

        /// <summary>
        /// Reads the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Object Read(int num)
        {
            byte[] b = new byte[num];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (byte)s.ReadByte();
            }
            Payload = Payload.Concat(b).ToArray();
            switch (num)
            {
                case 4:
                    return BitConverter.ToInt32(b, 0);
                case 8:
                    return BitConverter.ToInt64(b, 0);
                case 2:
                    return BitConverter.ToInt16(b, 0);
                default:
                    return 0;
            }
        }
    }
}
