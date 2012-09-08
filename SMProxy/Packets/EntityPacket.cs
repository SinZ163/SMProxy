using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EntityPacket : Packet
    {
        public int EntityId;

        public override byte PacketId
        {
            get { return 0x1E; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            return offset;
        }
    }
}
