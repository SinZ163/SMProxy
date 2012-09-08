using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class IncrementStatisticPacket : Packet
    {
        public int StatsticId;
        public byte Amount;

        public override byte PacketId
        {
            get { return 0xC8; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out StatsticId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Amount))
                return -1;
            return offset;
        }
    }
}
