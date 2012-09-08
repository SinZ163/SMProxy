using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class CreativeInventoryActionPacket : Packet
    {
        public short SlotIndex;
        public Slot ClickedItem;

        public override byte PacketId
        {
            get { return 0x6B; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out SlotIndex))
                return -1;
            if (!Slot.TryReadSlot(buffer, ref offset, out ClickedItem))
                return -1;
            return offset;
        }
    }
}
