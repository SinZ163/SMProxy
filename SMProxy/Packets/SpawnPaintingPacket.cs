﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class SpawnPaintingPacket : Packet
    {
        public int EntityId;
        public string Title;
        public Vector3 Position;
        public int Direction;

        public override byte PacketId
        {
            get { return 0x19; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int x, y, z;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Title))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out Direction))
                return -1;
            Position = new Vector3(x, y, z);
            return offset;
        }
    }
}
