using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SMProxy
{
    public class ProxySettings
    {
        public IPEndPoint LocalEndPoint { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public bool SingleSession { get; set; }
        public string FileName { get; set; }
        public bool LogClient { get; set; }
        public bool LogServer { get; set; }
        public bool EnableProfiling { get; set; }
        public byte[] FilterPackets { get; set; }
        public byte[] UnloggedPackets { get; set; }
        public List<byte> ServerSupressedPackets { get; set; }
        public List<byte> ClientSupressedPackets { get; set; }
        public string UserSession { get; set; }
        public string Username { get; set; }

        public ProxySettings()
        {
            RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 25565);
            LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 25564);
            SingleSession = LogClient = LogServer = true;
            EnableProfiling = false;
            ClientSupressedPackets = new List<byte>();
            ServerSupressedPackets = new List<byte>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Local endpoint: " + LocalEndPoint);
            sb.AppendLine("Remote endpoint: " + RemoteEndPoint);
            sb.AppendLine("Single session: " + SingleSession);
            sb.AppendLine("Log client traffic: " + LogClient);
            sb.AppendLine("Log server traffic: " + LogServer);
            sb.AppendLine("Profiling enabled: " + EnableProfiling);
            if (FilterPackets != null)
            {
                sb.Append("Filtered packets: ");
                foreach (var packetId in FilterPackets)
                    sb.AppendFormat("0x{0}, ", packetId.ToString("X2"));
                sb.Remove(sb.Length - 2, 2);
                sb.AppendLine();
            }
            if (UnloggedPackets != null)
            {
                sb.Append("Unlogged packets: ");
                foreach (var packetId in UnloggedPackets)
                    sb.AppendFormat("0x{0}, ", packetId.ToString("X2"));
                sb.Remove(sb.Length - 2, 2);
                sb.AppendLine();
            }
            if (ServerSupressedPackets.Count != 0)
            {
                sb.Append("Supressed client->server packets: ");
                foreach (var packetId in ServerSupressedPackets)
                    sb.AppendFormat("0x{0}, ", packetId.ToString("X2"));
                sb.Remove(sb.Length - 2, 2);
                sb.AppendLine();
            }
            if (ClientSupressedPackets.Count != 0)
            {
                sb.Append("Supressed server->client packets: ");
                foreach (var packetId in ClientSupressedPackets)
                    sb.AppendFormat("0x{0}, ", packetId.ToString("X2"));
                sb.Remove(sb.Length - 2, 2);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
