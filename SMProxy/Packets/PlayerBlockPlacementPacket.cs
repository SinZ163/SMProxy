using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class PlayerBlockPlacementPacket : Packet
    {
        public Vector3 Position;
        public byte Direction;
        public Slot HeldItem;
        public Vector3 Cursor;

        public override byte PacketId
        {
            get { return 0x0F; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int x, z;
            byte y, curX, curY, curZ;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Direction))
                return -1;
            if (!Slot.TryReadSlot(buffer, ref offset, out HeldItem))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out curX))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out curY))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out curZ))
                return -1;
            Position = new Vector3(x, y, z);
            Cursor = new Vector3(curX, curY, curZ);
            return offset;
        }
    }
}
