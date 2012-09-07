using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class AnimationPacket : Packet
    {
        public int EntityId;
        public byte Animation;

        public override byte PacketId
        {
            get { return 0x12; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Animation))
                return -1;
            return offset;
        }
    }
}
