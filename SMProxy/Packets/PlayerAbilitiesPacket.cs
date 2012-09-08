using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class PlayerAbilitiesPacket : Packet
    {
        public byte Flags;
        public byte FlyingSpeed;
        public byte WalkingSpeed;

        public override byte PacketId
        {
            get { return 0xCA; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Flags))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out FlyingSpeed))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out WalkingSpeed))
                return -1;
            return offset;
        }
    }
}
