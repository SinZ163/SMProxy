using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class InvalidPacket : Packet
    {
        public InvalidPacket(byte[] data)
        {
            packetId = data[0];
            Payload = data;
        }

        private byte packetId;
        public override byte PacketId { get { return packetId; } }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            throw new InvalidOperationException();
        }
    }
}
