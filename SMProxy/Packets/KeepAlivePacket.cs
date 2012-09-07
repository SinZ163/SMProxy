using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class KeepAlivePacket : Packet
    {
        [FieldDescription("The keep alive value, to be echoed to the server")]
        public int KeepAlive;

        public override byte PacketId
        {
            get { return 0x00; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out KeepAlive))
                return -1;
            return offset;
        }
    }
}
