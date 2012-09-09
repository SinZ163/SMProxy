using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class SpawnDroppedItemPacket : Packet
    {
        public int EntityId;
        public Slot Item;
        public Vector3 Position;
        public float Rotation;
        public float Pitch;
        public float Yaw;

        public override byte PacketId
        {
            get { return 0x15; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            double x, y, z;
            ushort id, metadata;
            byte count;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadUInt16(buffer, ref offset, out id))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out count))
                return -1;
            if (!DataUtility.TryReadUInt16(buffer, ref offset, out metadata))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Rotation))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Pitch))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Yaw))
                return -1;
            Item = new Slot(id, count, metadata);
            Position = new Vector3(x, y, z);
            return offset;
        }
    }
}
