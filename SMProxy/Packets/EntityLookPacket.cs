using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EntityLookPacket : Packet
    {
        public int EntityId;
        public float Yaw;
        public float Pitch;

        public override byte PacketId
        {
            get { return 0x20; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Yaw))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Pitch))
                return -1;
            return offset;
        }
    }
}
