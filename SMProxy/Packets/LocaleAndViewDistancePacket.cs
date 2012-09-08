using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class LocaleAndViewDistancePacket : Packet
    {
        public string Locale;
        public byte ViewDistance;
        public byte ChatFlags;
        public byte Difficulty;

        public override byte PacketId
        {
            get { return 0xCC; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Locale))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out ViewDistance))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out ChatFlags))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Difficulty))
                return -1;
            return offset;
        }
    }
}
