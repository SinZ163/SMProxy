using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class PlayerLookPacket : Packet
    {
        public float Yaw;
        public float Pitch;
        public bool OnGround;

        public override byte PacketId
        {
            get { return 0x0C; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Yaw))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Pitch))
                return -1;
            if (!DataUtility.TryReadBoolean(buffer, ref offset, out OnGround))
                return -1;
            return offset;
        }
    }
}
