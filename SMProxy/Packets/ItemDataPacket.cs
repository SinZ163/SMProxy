using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ItemDataPacket : Packet
    {
        public short ItemType;
        public short ItemId;
        public string Text;

        public override byte PacketId
        {
            get { return 0x83; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            byte textLength;
            byte[] text;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out ItemType))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out ItemId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out textLength))
                return -1;
            if (!DataUtility.TryReadArray(buffer, textLength, ref offset, out text))
                return -1;
            Text = Encoding.ASCII.GetString(text);
            return offset;
        }
    }
}
