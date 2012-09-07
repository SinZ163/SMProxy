using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class UpdateHealthPacket : Packet
    {
        public short Health;
        public short Food;
        public float FoodSaturation;

        public override byte PacketId
        {
            get { return 0x08; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out Health))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out Food))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out FoodSaturation))
                return -1;
            return offset;
        }
    }
}
