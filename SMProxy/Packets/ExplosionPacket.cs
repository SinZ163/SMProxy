using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class ExplosionPacket : Packet
    {
        public Vector3 Position;
        public float Radius;
        public int Records;
        public byte[] RecordData; // TODO: Improve
        [FriendlyName("[unknown]")]
        public float Unknown1;
        [FriendlyName("[unknown]")]
        public float Unknown2;
        [FriendlyName("[unknown]")]
        public float Unknown3;

        public override byte PacketId
        {
            get { return 0x3C; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            double x, y, z;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Radius))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out Records))
                return -1;
            if (!DataUtility.TryReadArray(buffer, (short)(Records * 3), ref offset, out RecordData))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Unknown1))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Unknown2))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Unknown3))
                return -1;
            return offset;
        }
    }
}
