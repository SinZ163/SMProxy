using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class SetSlotPacket : Packet
    {
        public byte WindowId;
        public short SlotIndex;
        public Slot Slot;

        public override byte PacketId
        {
            get { return 0x67; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out SlotIndex))
                return -1;
            if (!Slot.TryReadSlot(buffer, ref offset, out Slot))
                return -1;
            return offset;
        }
    }
}
