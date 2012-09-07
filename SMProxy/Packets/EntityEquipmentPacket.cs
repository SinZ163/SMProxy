using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EntityEquipmentPacket : Packet
    {
        public int EntityId;
        public short SlotIndex;
        public Slot Slot;

        public override byte PacketId
        {
            get { return 0x05; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out SlotIndex))
                return -1;
            if (!Slot.TryReadSlot(buffer, ref offset, out Slot))
                return -1;
            return offset;
        }
    }
}
