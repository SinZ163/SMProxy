using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class SetExperiencePacket : Packet
    {
        public float ExperienceBar;
        public short Level;
        public short TotalExperience;

        public override byte PacketId
        {
            get { return 0x2B; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out ExperienceBar))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out Level))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out TotalExperience))
                return -1;
            return offset;
        }
    }
}
