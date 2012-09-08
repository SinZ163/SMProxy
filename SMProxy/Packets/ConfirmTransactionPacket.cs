using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ConfirmTransactionPacket : Packet
    {
        public byte WindowId;
        public short ActionNumber;
        public bool Accepted;

        public override byte PacketId
        {
            get { return 0x6A; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WindowId))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out ActionNumber))
                return -1;
            if (!DataUtility.TryReadBoolean(buffer, ref offset, out Accepted))
                return -1;
            return offset;
        }
    }
}
