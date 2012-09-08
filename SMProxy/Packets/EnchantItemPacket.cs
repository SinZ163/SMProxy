using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EnchantItemPacket : Packet
    {
        public byte WindowId;
        public byte Enchantment;

        public override byte PacketId
        {
            get { return 0x6C; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Enchantment))
                return -1;
            return offset;
        }
    }
}
