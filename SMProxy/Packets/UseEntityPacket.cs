using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class UseEntityPacket : Packet
    {
        public int UserId;
        public int TargetId;
        public bool LeftClick;

        public override byte PacketId
        {
            get { return 0x07; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out UserId))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out TargetId))
                return -1;
            if (!DataUtility.TryReadBoolean(buffer, ref offset, out LeftClick))
                return -1;
            return offset;
        }
    }
}
