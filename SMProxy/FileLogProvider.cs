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

        public FileLogProvider(string file)
        {
            stream = new StreamWriter(file);
        }

        public void Log(string text)
        {
            stream.WriteLine(text);
            stream.Flush();
        }

        public void Log(Packet packet, Proxy proxy)
        {
            StringBuilder sb = new StringBuilder();
            if (packet.PacketContext == PacketContext.ClientToServer)
                sb.Append("{" + DateTime.Now.ToShortTimeString() + "} [CLIENT " + proxy.RemoteSocket.RemoteEndPoint + "->SERVER]: ");
            else
                sb.Append("{" + DateTime.Now.ToShortTimeString() + "} [SERVER->CLIENT " + proxy.LocalSocket.RemoteEndPoint + "]: ");
            sb.Append(packet.GetType().Name.Replace("Packet", ""));
            sb.AppendFormat(" (0x{0})", packet.PacketId.ToString("X2"));
            sb.AppendLine();
            sb.Append(DataUtility.DumpArrayPretty(packet.Payload));
            sb.AppendLine(packet.ToString());
            stream.WriteLine(sb.ToString());
            stream.Flush();
        }

        public void Raw(byte[] payload, Proxy proxy, PacketContext packetContext)
        {
            StringBuilder sb = new StringBuilder();
            if (packetContext == PacketContext.ClientToServer)
                sb.AppendLine("RAW {" + DateTime.Now.ToShortTimeString() + "} [CLIENT " + proxy.RemoteSocket.RemoteEndPoint + "->SERVER]: ");
            else
                sb.AppendLine("RAW {" + DateTime.Now.ToShortTimeString() + "} [SERVER->CLIENT " + proxy.LocalSocket.RemoteEndPoint + "]: ");
            sb.AppendLine(DataUtility.DumpArrayPretty(payload));
            stream.Write(sb.ToString());
            stream.Flush();
        }
    }
}
