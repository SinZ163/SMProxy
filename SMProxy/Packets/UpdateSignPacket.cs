using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class UpdateSignPacket : Packet
    {
        public Vector3 Position;
        public string Line1;
        public string Line2;
        public string Line3;
        public string Line4;

        public override byte PacketId
        {
            get { return 0x82; }
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
            if (!DataUtility.TryReadString(buffer, ref offset, out Line1))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Line2))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Line3))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Line4))
                return -1;
            Position = new Vector3(x, y, z);
            return offset;
        }
    }
}
