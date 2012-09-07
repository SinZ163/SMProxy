using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class PlayerPositionPacket : Packet
    {
        public Vector3 Position;
        public double Stance;
        public bool OnGround;

        public override byte PacketId
        {
            get { return 0x0B; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            double x, y, z;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out Stance))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadBoolean(buffer, ref offset, out OnGround))
                return -1;
            Position = new Vector3(x, y, z);
            return offset;
        }
    }
}
