using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ChangeGameStatePacket : Packet
    {
        public byte Reason;
        public byte GameMode;

        public override byte PacketId
        {
            get { return 0x46; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Reason))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out GameMode))
                return -1;
            return offset;
        }
    }
}
