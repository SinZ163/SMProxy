using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class UpdateWindowPropertyPacket : Packet
    {
        public byte WindowId;
        public short Property;
        public short Value;

        public override byte PacketId
        {
            get { return 0x69; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out Property))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out Value))
                return -1;
            return offset;
        }
    }
}
