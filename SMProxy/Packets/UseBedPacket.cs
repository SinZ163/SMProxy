using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class UseBedPacket : Packet
    {
        public int EntityId;
        [FriendlyName("[unknown]")]
        public byte Unknown;
        public Vector3 Position;

        public override byte PacketId
        {
            get { return 0x11; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int x, z;
            byte y;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Unknown))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            Position = new Vector3(x, y, z);
            return offset;
        }
    }
}
