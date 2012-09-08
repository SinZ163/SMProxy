using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Craft.Net.Data.Metadata;

namespace SMProxy.Packets
{
    public class SpawnNamedEntityPacket : Packet
    {
        public int EntityId;
        public string PlayerName;
        public Vector3 Position;
        public float Yaw;
        public float Pitch;
        public short CurrentItem;
        public MetadataDictionary Metadata;

        public override byte PacketId
        {
            get { return 0x14; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            double x, y, z;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out PlayerName))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadAbsoluteInteger(buffer, ref offset, out z))
                return -1;
            if (!MetadataDictionary.TryReadMetadata(buffer, ref offset, out Metadata))
                return -1;
            return offset;
        }
    }
}
