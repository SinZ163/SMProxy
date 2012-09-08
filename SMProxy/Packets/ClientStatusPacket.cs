using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ClientStatusPacket : Packet
    {
        public byte Status;

        public override byte PacketId
        {
            get { return 0xCD; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Status))
                return -1;
            return offset;
        }
    }
}
