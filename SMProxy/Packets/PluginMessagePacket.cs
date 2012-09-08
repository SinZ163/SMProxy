using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class PluginMessagePacket : Packet
    {
        public string Channel;
        public byte[] Data;

        public override byte PacketId
        {
            get { return 0xFA; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            short dataLength;
            if (!DataUtility.TryReadString(buffer, ref offset, out Channel))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out dataLength))
                return -1;
            if (!DataUtility.TryReadArray(buffer, dataLength, ref offset, out Data))
                return -1;
            return offset;
        }
    }
}
