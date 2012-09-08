using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EntityVelocityPacket : Packet
    {
        public int EntityId;
        public Vector3 Velocity;

        public override byte PacketId
        {
            get { return 0x1C; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            short x, y, z;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out z))
                return -1;
            double velX = x / 32000;
            double velY = y / 32000;
            double velZ = z / 32000;
            Velocity = new Vector3(velX, velY, velZ);
            return offset;
        }
    }
}
