using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class CollectItemPacket : Packet
    {
        public int ItemId;
        public int PlayerId;
        
        public override byte PacketId
        {
            get { return 0x16; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out ItemId))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out PlayerId))
                return -1;
            return offset;
        }
    }
}
