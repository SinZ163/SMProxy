using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class MapChunkBulkPacket : Packet // TODO: Make this packet more detailed
    {
        public byte[] CompressedData;
        public byte[] ChunkMetadata;

        public override byte PacketId
        {
            get { return 0x38; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            short chunks;
            int dataLength;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out chunks))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out dataLength))
                return -1;
            if (!DataUtility.TryReadArray(buffer, (short)dataLength, ref offset, out CompressedData))
                return -1;
            if (!DataUtility.TryReadArray(buffer, (short)(chunks * 12), ref offset, out ChunkMetadata))
                return -1;
            return offset;
        }
    }
}
