using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        // Metadata

        public override byte PacketId
        {
            get { return 0x14; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }
    }
}
