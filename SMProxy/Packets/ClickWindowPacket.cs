using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ClickWindowPacket : Packet
    {
        public byte WindowId;
        public short SlotIndex;
        public bool RightClick;
        public short ActionNumber;
        public bool Shift;
        public Slot ClickedItem;

        public override byte PacketId
        {
            get { return 0x66; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out SlotIndex))
                return -1;
            if (!DataUtility.TryReadBoolean(buffer, ref offset, out RightClick))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out ActionNumber))
                return -1;
            if (!DataUtility.TryReadBoolean(buffer, ref offset, out Shift))
                return -1;
            if (!Slot.TryReadSlot(buffer, ref offset, out ClickedItem))
                return -1;
            return offset;
        }
    }
}
