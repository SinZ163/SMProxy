using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class DestroyEntityPacket : Packet
    {
        public int[] Entities;

        public override byte PacketId
        {
            get { return 0x1D; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            byte count;
            if (!DataUtility.TryReadByte(buffer, ref offset, out count))
                return -1;
            if (buffer.Length < (count * 4) + 1)
                return -1;
            Entities = new int[count];
            for (int i = 0; offset < (count * 4) + 1; i++)
                DataUtility.TryReadInt32(buffer, ref offset, out Entities[i]);
            return offset;
        }
    }
}
