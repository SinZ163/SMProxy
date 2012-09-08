using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class EntityEffectPacket : Packet
    {
        public int EntityId;
        public int EffectId;
        public byte Amplifier;
        public short Duration;

        public override byte PacketId
        {
            get { return 0x29; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EffectId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Amplifier))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out Duration))
                return -1;
            return offset;
        }
    }
}
