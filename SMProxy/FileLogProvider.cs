using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SMProxy
{
    public class FileLogProvider : ILogProvider
    {
        private StreamWriter stream;
        private ProxySettings settings;

        public FileLogProvider(ProxySettings settings, string file)
        {
            stream = new StreamWriter(file, true);
            this.settings = settings;
            stream.WriteLine("Log opened on " + DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString());
            stream.WriteLine("Settings:");
            stream.WriteLine(settings.ToString());
        }

        public void Log(string text)
        {
            stream.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} " + text + Environment.NewLine);
            stream.Flush();
        }

        public void Log(Packet packet, Proxy proxy)
        {
            if (packet.PacketContext == PacketContext.ServerToClient && !settings.LogServer)
                return;
            if (packet.PacketContext == PacketContext.ClientToServer && !settings.LogClient)
                return;
            if (settings.FilterPackets != null)
            {
                if (!settings.FilterPackets.Contains(packet.PacketId))
                    return;
            }
            if (settings.UnloggedPackets != null)
            {
                if (settings.FilterPackets.Contains(packet.PacketId))
                    return;
            }

            StringBuilder sb = new StringBuilder();
            if (packet.PacketContext == PacketContext.ClientToServer)
                sb.Append("{" + DateTime.Now.ToLongTimeString() + "} [CLIENT " + proxy.RemoteSocket.RemoteEndPoint + "->SERVER]: ");
            else
                sb.Append("{" + DateTime.Now.ToLongTimeString() + "} [SERVER->CLIENT " + proxy.LocalSocket.RemoteEndPoint + "]: ");

            sb.Append(Packet.AddSpaces(packet.GetType().Name.Replace("Packet", "")));
            sb.AppendFormat(" (0x{0})", packet.PacketId.ToString("X2"));
            sb.AppendLine();
            sb.Append(DataUtility.DumpArrayPretty(packet.Payload));
            sb.AppendLine(packet.ToString(proxy));

            stream.Write(sb + Environment.NewLine);
            stream.Flush();
        }

        public void Raw(byte[] payload, Proxy proxy, PacketContext packetContext)
        {
            if (packetContext == PacketContext.ServerToClient && !settings.LogServer)
                return;
            if (packetContext == PacketContext.ClientToServer && !settings.LogClient)
                return;
            StringBuilder sb = new StringBuilder();
            if (packetContext == PacketContext.ClientToServer)
                sb.AppendLine("RAW {" + DateTime.Now.ToLongTimeString() + "} [CLIENT " + proxy.RemoteSocket.RemoteEndPoint + "->SERVER]: ");
            else
                sb.AppendLine("RAW {" + DateTime.Now.ToLongTimeString() + "} [SERVER->CLIENT " + proxy.LocalSocket.RemoteEndPoint + "]: ");
            sb.Append(DataUtility.DumpArrayPretty(payload));
            stream.Write(sb + Environment.NewLine);
            stream.Flush();
        }
    }
}
