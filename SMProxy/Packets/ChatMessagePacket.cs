using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ChatMessagePacket : Packet
    {
        public string Message;

        public override byte PacketId
        {
            get { return 0x03; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Message))
                return -1;
            return offset;
        }
    }
}
