using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class HandshakePacket : Packet
    {
        public byte ProtocolVersion;
        public string Username;
        public string Hostname;
        public int ServerPort;

        public override byte PacketId
        {
            get { return 0x2; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out ProtocolVersion))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Username))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Hostname))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out ServerPort))
                return -1;
            return offset;
        }

        public override void HandlePacket(Proxy proxy)
        {
            if (ProtocolVersion != Proxy.ProtocolVersion)
                proxy.LogProvider.Log("WARNING: Protocol version provided by client does not match SMProxy version (" + Proxy.ProtocolVersion + ")");
            proxy.Settings.Username = Username;
        }
    }
}
