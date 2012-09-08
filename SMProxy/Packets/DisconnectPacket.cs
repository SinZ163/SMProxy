using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class DisconnectPacket : Packet
    {
        public string Reason;

        public override byte PacketId
        {
            get { return 0xFF; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Reason))
                return -1;
            return offset;
        }
    }
}
