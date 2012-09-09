using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class MultiBlockChangePacket : Packet
    {
        public int ChunkX;
        public int ChunkZ;
        public short BlocksAffected;
        public byte[] Data; // TODO: Make this better

        public override byte PacketId
        {
            get { return 0x34; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int dataLength;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out ChunkX))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out ChunkZ))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out BlocksAffected))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out dataLength))
                return -1;
            if (!DataUtility.TryReadArray(buffer, dataLength, ref offset, out Data))
                return -1;
            return offset;
        }
    }
}
