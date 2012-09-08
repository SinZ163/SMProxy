using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class TabCompletePacket : Packet
    {
        public string Text;

        public override byte PacketId
        {
            get { return 0xCB; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Text))
                return -1;
            return offset;
        }
    }
}
