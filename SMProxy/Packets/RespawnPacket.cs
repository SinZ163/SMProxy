using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class RespawnPacket : Packet
    {
        public Dimension Dimension;
        public Difficulty Difficulty;
        public GameMode GameMode;
        public short WorldHeight;
        public string LevelType;

        public override byte PacketId
        {
            get { return 0x09; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            int dimension;
            byte difficulty, gameMode;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out dimension))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out difficulty))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out gameMode))
                return -1;
            if (!DataUtility.TryReadInt16(buffer, ref offset, out WorldHeight))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out LevelType))
                return -1;
            Dimension = (Dimension)dimension;
            Difficulty = (Difficulty)difficulty;
            GameMode = (GameMode)gameMode;
            return offset;
        }
    }
}
