using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class NamedSoundEffectPacket : Packet
    {
        public string SoundName;
        public Vector3 Position;
        public float Volume;
        public byte Pitch;

        public override byte PacketId
        {
            get { return 0x3E; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int x, y, z;
            if (!DataUtility.TryReadString(buffer, ref offset, out SoundName))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Volume))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Pitch))
                return -1;
            return offset;
        }
    }
}
