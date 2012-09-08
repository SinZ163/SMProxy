using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ServerListPingPacket : Packet
    {
        public override byte PacketId
        {
            get { return 0xFE; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            return 1;
        }
    }
}
