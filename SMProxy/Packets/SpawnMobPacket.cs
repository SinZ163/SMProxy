using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Craft.Net.Data.Metadata;

namespace SMProxy.Packets
{
    public class SpawnMobPacket : Packet
    {
        public int EntityId;
        public byte Type;
        public Vector3 Position;
        public float Yaw;
        public float Pitch;
        public float HeadYaw;
        public Vector3 Velocity;
        public MetadataDictionary Metadata;

        public override byte PacketId
        {
            get { return 0x18; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int x, y, z;
            short velX, velY, velZ;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Type))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Yaw))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out Pitch))
                return -1;
            if (!DataUtility.TryReadPackedByte(buffer, ref offset, out HeadYaw))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out velX))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out velY))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out velZ))
                return -1;
            if (!MetadataDictionary.TryReadMetadata(buffer, ref offset, out Metadata))
                return -1;
            Position = new Vector3(x, y, z);
            Velocity = new Vector3(velX, velY, velZ);
            return offset;
        }
    }
}
