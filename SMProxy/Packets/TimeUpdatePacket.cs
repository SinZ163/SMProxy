using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class TimeUpdatePacket : Packet
    {
        public long Time;

        public override byte PacketId
        {
            get { return 0x04; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt64(buffer, ref offset, out Time))
                return -1;
            return offset;
        }
    }
}
