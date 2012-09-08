using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EntityLookAndRelativeMovePacket : Packet
    {
        public int EntityId;
        public Vector3 Delta;
        public float Yaw;
        public float Pitch;

        public override byte PacketId
        {
            get { return 0x21; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            double dX, dY, dZ;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out dX))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out dY))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out dZ))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Yaw))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Pitch))
                return -1;
            Delta = new Vector3(dX, dY, dZ);
            return offset;
        }
    }
}
