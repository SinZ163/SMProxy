using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy.Packets
{
    public class LoginRequestPacket : Packet
    {
        [FieldDescription("This client's entity ID")]
        public int EntityId;
        [FieldDescription("The terrain generation type for this level")]
        public string LevelType;
        [FieldDescription("The connecting player's game mode")]
        public GameMode GameMode;
        [FieldDescription("The connecting player's dimension")]
        public Dimension Dimension;
        [FieldDescription("The difficulty of the server")]
        public Difficulty Difficulty;
        [FieldDescription("The maximum players this server allows at once (for drawing the player list)")]
        public byte MaxPlayers;
        [FriendlyName("[discarded]")]
        public byte Discarded;

        public override byte PacketId
        {
            get { return 0x01; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            byte gameMode, dimension, difficulty, discarded;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out EntityId))
                return -1;
            if (!DataUtility.TryReadString(buffer, ref offset, out LevelType))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out gameMode))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out dimension))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out difficulty))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out discarded))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out MaxPlayers))
                return -1;
            GameMode = (GameMode)gameMode;
            Dimension = (Dimension)dimension;
            Difficulty = (Difficulty)difficulty;
            return offset;
        }
    }
}
