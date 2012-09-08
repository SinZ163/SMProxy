using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Craft.Net.Data.Metadata;

namespace SMProxy.Packets
{
    public class EntityMetadataPacket : Packet
    {
        public int EntityId;
        public MetadataDictionary Metadata;

        public override byte PacketId
        {
            get { return 0x28; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!MetadataDictionary.TryReadMetadata(buffer, ref offset, out Metadata))
                return -1;
            return offset;
        }
    }
}
