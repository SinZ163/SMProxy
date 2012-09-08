using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class BlockActionPacket : Packet
    {
        public Vector3 Position;
        public byte Data1;
        public byte Data2;
        public short BlockId;

        public override byte PacketId
        {
            get { return 0x36; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int x, z;
            short y;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Data1))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Data2))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out BlockId))
                return -1;
            return offset;
        }
    }
}
