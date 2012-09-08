using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EntityHeadLookPacket : Packet
    {
        public int EntityId;
        public float HeadYaw;

        public override byte PacketId
        {
            get { return 0x23; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out HeadYaw))
                return -1;
            return offset;
        }
    }
}
