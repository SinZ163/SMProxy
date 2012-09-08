using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class CloseWindowPacket : Packet
    {
        public byte WindowId;

        public override byte PacketId
        {
            get { return 0x65; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            return offset;
        }
    }
}
