using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class SoundOrParticleEffectPacket : Packet
    {
        public int EffectId;
        public Vector3 Position;
        public int Data;

        public override byte PacketId
        {
            get { return 0x3D; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int x, z;
            byte y;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EffectId))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out Data))
                return -1;
            Position = new Vector3(x, y, z);
            return offset;
        }
    }
}
