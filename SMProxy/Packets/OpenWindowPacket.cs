using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class OpenWindowPacket : Packet
    {
        public byte WindowId;
        public byte InventoryType;
        public string WindowTitle;
        public byte Slots;

        public override byte PacketId
        {
            get { return 0x64; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out InventoryType))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out WindowTitle))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Slots))
                return -1;
            return offset;
        }
    }
}
