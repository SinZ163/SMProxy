using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LuaInterface;
using NCalc;

namespace SMProxy
{
    static partial class Program
    {
        static bool LuaError = false;

        private static void HandleConnection(StreamWriter outputLogger, TcpClient client, string ServerAddress, int RemotePort)
        {
            TcpClient server = null;

            bool ClientDirty = false, ServerDirty = false;
            Console.WriteLine("Connected to remote server.");

            try
            {
                while (client.Connected && (server == null || server.Connected))
                {
                    if (client.Available != 0)
                    {
                        DateTime downloadStartTime = DateTime.Now;
                        PacketReader pr = new PacketReader(client);

                        Lua lua = null;
                        if (Type.GetType("Mono.Runtime") == null)
                            lua = ConfigureLua(pr, outputLogger);
                        else if (!LuaError)
                        {
                            LuaError = true;
                            Console.WriteLine("WARNING!  Lua scripts are not currently supported on Mono, and have been disabled.");
                        }

                        byte data = pr.ReadByte();
                        if (data != 0x02 && server == null)
                            server = new TcpClient(ServerAddress, RemotePort);

                        if (ClientDirty)
                        {
                            try
                            {
                                server.GetStream().WriteByte(data);
                                outputLogger.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [RAW CLIENT->SERVER]: " + ((byte)data).ToString("x"));
                            }
                            catch { }
                        }
                        else
                        {
                            bool suppressed = ServerDenyPackets.Contains(data);
                            try
                            {
                                if (CustomClientScripts.ContainsKey(data))
                                {
                                    if (lua != null)
                                    {
                                        string[] customPacket = CustomClientScripts[data].Split(':');
                                        StreamReader reader = new StreamReader(customPacket[1]);
                                        string script = reader.ReadToEnd();
                                        reader.Close();
                                        lua.DoString(script);

                                        LogPacket(outputLogger, true, data, customPacket[0], pr);
                                    }
                                }
                                else if (CustomClientPackets.ContainsKey(data))
                                {
                                    string[] customPacket = CustomClientPackets[(byte)data].Split(':');
                                    List<object> packetData = new List<object>();
                                    foreach (string item in customPacket[1].Split(','))
                                    {
                                        string parameter = new string(item.ToCharArray());
                                        string type = parameter;
                                        string name = parameter;
                                        string expression = parameter;
                                        if (parameter.Contains("[") && parameter.Contains("]"))
                                        {
                                            expression = parameter.Substring(item.IndexOf("[") + 1);
                                            expression = expression.Remove(expression.IndexOf("]"));
                                            parameter = parameter.Remove(parameter.IndexOf("["), parameter.IndexOf("]") - parameter.IndexOf("[") + 1);
                                            type = parameter;
                                        }
                                        if (parameter.Contains("(") && parameter.Contains(")"))
                                        {
                                            type = parameter.Remove(parameter.IndexOf("("));
                                            name = parameter.Substring(name.IndexOf("(") + 1);
                                            name = name.Remove(name.IndexOf(")"));
                                        }
                                        packetData.Add(name);
                                        switch (type)
                                        {
                                            case "boolean":
                                                packetData.Add(pr.ReadBoolean());
                                                break;
                                            case "byte":
                                                packetData.Add(pr.ReadByte());
                                                break;
                                            case "short":
                                                packetData.Add(pr.ReadShort());
                                                break;
                                            case "int":
                                                packetData.Add(pr.ReadInt());
                                                break;
                                            case "long":
                                                packetData.Add(pr.ReadLong());
                                                break;
                                            case "float":
                                                packetData.Add(pr.ReadFloat());
                                                break;
                                            case "double":
                                                packetData.Add(pr.ReadDouble());
                                                break;
                                            case "slot":
                                                packetData.Add(pr.ReadSlot());
                                                break;
                                            case "mob":
                                                packetData.Add(pr.ReadMobMetadata());
                                                break;
                                            case "string":
                                                packetData.Add(pr.ReadString());
                                                break;
                                            case "array":
                                                Dictionary<string, object> evalParams = new Dictionary<string, object>();
                                                for (int i = 0; i < packetData.Count - 1; i += 2)
                                                {
                                                    if (packetData[i + 1] is byte ||
                                                        packetData[i + 1] is short ||
                                                        packetData[i + 1] is int ||
                                                        packetData[i + 1] is long &&
                                                        !evalParams.ContainsKey(packetData[i].ToString()))
                                                    {
                                                        evalParams.Add(packetData[i].ToString(), packetData[i + 1]);
                                                        expression = expression.Replace(packetData[i].ToString(), "[" + packetData[i].ToString() + "]");
                                                    }
                                                }
                                                Expression exp = new Expression(expression);
                                                exp.Parameters = evalParams;
                                                var result = exp.Evaluate();
                                                packetData.Add(pr.ReadBytes((int)(double.Parse(result.ToString())))); // Please excuse this, CIL can be ridiculous sometimes
                                                break;
                                        }
                                    }
                                    LogPacket(outputLogger, true, (byte)data, customPacket[0], pr, packetData.ToArray());
                                }
                                else
                                {
                                    // Client to Server
                                    #region Client to Server
                                    switch (data)
                                    {
                                        case 0x00: // Keep-alive
                                            LogPacket(outputLogger, true, 0x00, pr,
                                                "Keep-Alive", pr.ReadInt());
                                            break;
                                        case 0x01: // Login Request
                                            LogPacket(outputLogger, true, 0x01, pr,
                                                "Protocol Version", pr.ReadInt(ProtocolVersion),
                                                "Username", pr.ReadString());
                                            pr.ReadString();
                                            pr.Read(11);
                                            break;
                                        case 0x02: // Handshake
                                            string usernameAndHost = pr.ReadString();
                                            LogPacket(outputLogger, true, 0x02, pr,
                                                "Username/Hostname", usernameAndHost);
                                            if (usernameAndHost.Contains(";"))
                                            {
                                                string[] parts = usernameAndHost.Split(';');
                                                string[] host = parts[1].Split(':');
                                                int port = 25565;
                                                if (host.Length != 1)
                                                    port = int.Parse(host[1]);
                                                if (VirtualHosts.ContainsKey(host[0] + ":" + port.ToString()))
                                                {
                                                    host = VirtualHosts[host[0] + ":" + port.ToString()].Split(':');
                                                    port = 25565;
                                                    if (host.Length != 1)
                                                        port = int.Parse(host[1]);
                                                    server = new TcpClient(host[0], port);
                                                }
                                                else
                                                    server = new TcpClient(ServerAddress, RemotePort);
                                            }
                                            else
                                                server = new TcpClient(ServerAddress, RemotePort);
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
                                        case 0xCA: // Player Abilities
                                            LogPacket(outputLogger, true, 0xCA, pr,
                                                "Invulnerable", pr.ReadBoolean(),
                                                "Is Flying", pr.ReadBoolean(),
                                                "Can Fly", pr.ReadBoolean(),
                                                "Instant Mine", pr.ReadBoolean());
                                            break;
                                        case 0xCB: // Tab Complete
                                            LogPacket(outputLogger, true, 0xCB, pr,
                                                "Text", pr.ReadString());
                                            break;
                                        case 0xCC: // Locale and View Distance
                                            LogPacket(outputLogger, true, 0xCC, pr,
                                                "Locale", pr.ReadString(),
                                                "View Distance", pr.ReadByte(),
                                                "Chat Flags", pr.ReadByte(),
                                                "Difficulty", pr.ReadByte());
                                            break;
                                        case 0xCD: // Client Status
                                            LogPacket(outputLogger, true, 0xCD, pr,
                                                "Payload", pr.ReadByte());
                                            break;
                                        case 0xFA: // Plugin Message
                                            string s = pr.ReadString();
                                            short l = pr.ReadShort();
                                            LogPacket(outputLogger, true, 0xFA, pr,
                                                "Channel", s,
                                                "Length", l,
                                                "Data", pr.Read(l));
                                            break;
                                        case 0xFC: // Encryption Key Response
                                            short keyLength = pr.ReadShort();
                                            byte[] key = pr.ReadBytes(keyLength);
                                            short verifyLength = pr.ReadShort();
                                            LogPacket(outputLogger, true, 0xFC, pr,
                                                 "Key Length", keyLength,
                                                 "Key", key,
                                                 "Verify Token Length", verifyLength,
                                                 "Verify Token", pr.ReadBytes(verifyLength));
                                            // TODO: Handle encryption
                                            break;
                                        case 0xFE: // Server List Ping
                                            LogPacket(outputLogger, true, 0xFE, pr);
                                            break;
                                        case 0xFF: // Disconnect
                                            LogPacket(outputLogger, true, 0xFF, pr,
                                                "Reason", pr.ReadString());
                                            server.GetStream().Write(pr.Payload, 0, pr.Payload.Length);
                                            client.Close();
                                            server.Close();
                                            break;
                                        default:
                                            ClientDirty = true;
                                            Console.WriteLine("WARNING: Client send unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                            outputLogger.WriteLine("WARNING: Client sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                            break;
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                ClientDirty = true;
                                Console.WriteLine("Exception occured:");
                                Console.WriteLine(e.ToString());
                                Console.WriteLine("WARNING: Client sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                outputLogger.WriteLine("WARNING: Client sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                            }
                            finally
                            {
                                DateTime downloadCompleteTime = DateTime.Now;
                                DateTime uploadStartTime = DateTime.Now;
                                if (server.Connected && !suppressed)
                                    server.GetStream().Write(pr.Payload, 0, pr.Payload.Length);

                                LogProfiling(outputLogger, downloadStartTime, downloadCompleteTime, uploadStartTime, true,
                                    (byte)data, pr);
                            }
                        }
                    }
                    if (server != null && server.Connected && client.Connected && server.Available != 0)
                    {
                        DateTime downloadStartTime = DateTime.Now;

                        PacketReader pr = new PacketReader(server);

                        byte data = pr.ReadByte();

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
                            bool suppressed = ClientDenyPackets.Contains((byte)data);
                            try
                            {
                                if (CustomServerPackets.ContainsKey((byte)data))
                                {
                                    string[] customPacket = CustomServerPackets[(byte)data].Split(':');
                                    List<object> packetData = new List<object>();
                                    foreach (string item in customPacket[1].Split(','))
                                    {
                                        string parameter = new string(item.ToCharArray());
                                        string type = parameter;
                                        string name = parameter;
                                        string expression = parameter;
                                        if (parameter.Contains("[") && parameter.Contains("]"))
                                        {
                                            expression = parameter.Substring(item.IndexOf("[") + 1);
                                            expression = expression.Remove(expression.IndexOf("]"));
                                            parameter = parameter.Remove(parameter.IndexOf("["), parameter.IndexOf("]") - parameter.IndexOf("[") + 1);
                                            type = parameter;
                                        }
                                        if (parameter.Contains("(") && parameter.Contains(")"))
                                        {
                                            type = parameter.Remove(parameter.IndexOf("("));
                                            name = parameter.Substring(name.IndexOf("(") + 1);
                                            name = name.Remove(name.IndexOf(")"));
                                        }
                                        packetData.Add(name);
                                        switch (type)
                                        {
                                            case "boolean":
                                                packetData.Add(pr.ReadBoolean());
                                                break;
                                            case "byte":
                                                packetData.Add(pr.ReadByte());
                                                break;
                                            case "short":
                                                packetData.Add(pr.ReadShort());
                                                break;
                                            case "int":
                                                packetData.Add(pr.ReadInt());
                                                break;
                                            case "long":
                                                packetData.Add(pr.ReadLong());
                                                break;
                                            case "float":
                                                packetData.Add(pr.ReadFloat());
                                                break;
                                            case "double":
                                                packetData.Add(pr.ReadDouble());
                                                break;
                                            case "slot":
                                                packetData.Add(pr.ReadSlot());
                                                break;
                                            case "mob":
                                                packetData.Add(pr.ReadMobMetadata());
                                                break;
                                            case "string":
                                                packetData.Add(pr.ReadString());
                                                break;
                                            case "array":
                                                Dictionary<string, object> evalParams = new Dictionary<string, object>();
                                                for (int i = 0; i < packetData.Count - 1; i += 2)
                                                {
                                                    if ((packetData[i + 1] is byte ||
                                                        packetData[i + 1] is short ||
                                                        packetData[i + 1] is int ||
                                                        packetData[i + 1] is long) &&
                                                        !evalParams.ContainsKey(packetData[i].ToString()))
                                                    {
                                                        evalParams.Add(packetData[i].ToString(), packetData[i + 1]);
                                                        expression = expression.Replace(packetData[i].ToString(), "[" + packetData[i].ToString() + "]");
                                                    }
                                                }
                                                Expression exp = new Expression(expression);
                                                exp.Parameters = evalParams;
                                                var result = exp.Evaluate();
                                                packetData.Add(pr.ReadBytes((int)(double.Parse(result.ToString())))); // Please excuse this, CIL can be ridiculous sometimes
                                                break;
                                        }
                                    }
                                    LogPacket(outputLogger, false, (byte)data, customPacket[0], pr, packetData.ToArray());
                                }
                                else
                                {
                                    // Server to Client
                                    #region Server to Client
                                    switch (data)
                                    {
                                        case 0x00: // Keep-alive
                                            LogPacket(outputLogger, false, 0x00, pr,
                                                "Keep-Alive", pr.ReadInt());
                                            break;
                                        case 0x01: // Login Request
                                            LogPacket(outputLogger, false, 0x01, pr,
                                                "Entity ID", pr.ReadInt(),
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
                                        case 0x11: // Use Bed
                                            LogPacket(outputLogger, false, 0x11, pr,
                                                "Entity ID", pr.ReadInt(),
                                                "In Bed?", pr.ReadByte(),
                                                "X", pr.ReadInt(),
                                                "Y", pr.ReadByte(),
                                                "Z", pr.ReadInt());
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
                                        case 0xCA: // Player Abilities
                                            LogPacket(outputLogger, true, 0xCA, pr,
                                                "Invulnerable", pr.ReadBoolean(),
                                                "Is Flying", pr.ReadBoolean(),
                                                "Can Fly", pr.ReadBoolean(),
                                                "Instant Mine", pr.ReadBoolean());
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
                                            client.GetStream().Write(pr.Payload, 0, pr.Payload.Length);
                                            server.Close();
                                            client.Close();
                                            break;
                                        default:
                                            ServerDirty = true;
                                            Console.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                            outputLogger.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                            break;
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                ServerDirty = true;
                                Console.WriteLine("Exception occured:");
                                Console.WriteLine(e.ToString());
                                Console.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                                outputLogger.WriteLine("WARNING: Server sent unrecognized packet (0x" + data.ToString("x") + ")!  Switching to raw log mode.");
                            }
                            finally
                            {
                                DateTime downloadCompleteTime = DateTime.Now;
                                DateTime uploadStartTime = DateTime.Now;
                                if (client.Connected && !suppressed)
                                    client.GetStream().Write(pr.Payload, 0, pr.Payload.Length);

                                LogProfiling(outputLogger, downloadStartTime, downloadCompleteTime, uploadStartTime, false,
                                    (byte)data, pr);
                            }
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch 
            {
                Console.WriteLine("WARNING: Exception occured while processing client.");
            }

            Console.WriteLine("Disconnected.");
        }
    }
}
