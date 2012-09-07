using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class HeldItemChangePacket : Packet
    {
        public short SlotIndex;

        public override byte PacketId
        {
            get { return 0x10; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out SlotIndex))
                return -1;
            return offset;
        }
    }
}
