using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class SetWindowItemsPacket : Packet
    {
        public byte WindowId;
        public Slot[] Slots;

        public override byte PacketId
        {
            get { return 0x68; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            short count;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out count))
                return -1;
            Slots = new Slot[count];
            for (int i = 0; i < count; i++ )
            {
                if (!Slot.TryReadSlot(buffer, ref offset, out Slots[i]))
                    return -1;
            }
            return offset;
        }
    }
}
