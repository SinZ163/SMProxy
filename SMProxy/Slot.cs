using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibMinecraft.Model.Packets;
using System.IO.Compression;
using LibNbt;
using SMProxy;

namespace LibMinecraft.Model
{
    /// <summary>
    /// Represents an inventory slot.
    /// </summary>
    /// <remarks></remarks>
    public class Slot
    {
        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        /// <value>The item ID.</value>
        /// <remarks>This ID may be a block or an item.</remarks>
        public short ID { get; set; }
        /// <summary>
        /// Gets or sets the item count.
        /// </summary>
        /// <value>The item count.</value>
        /// <remarks></remarks>
        public byte Count { get; set; }
        /// <summary>
        /// Gets or sets the item metadata.
        /// </summary>
        /// <value>The item metadata.</value>
        /// <remarks></remarks>
        public short Metadata { get; set; }
        /// <summary>
        /// Gets or sets the NBT data.
        /// </summary>
        /// <value>The NBT data.</value>
        /// <remarks>This is used for enchanting equipment</remarks>
        public NbtFile Nbt { get; set; }

        public byte[] Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Slot"/> class.
        /// </summary>
        /// <remarks></remarks>
        public Slot()
        {
            this.ID = 0;
            this.Count = 1;
            this.Metadata = 0;
            this.Nbt = new NbtFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Slot"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="Count">The count.</param>
        /// <remarks></remarks>
        public Slot(short ID, byte Count)
        {
            this.ID = ID;
            this.Count = Count;
            this.Metadata = 0;
            this.Nbt = new NbtFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Slot"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="Count">The count.</param>
        /// <param name="Metadata">The metadata.</param>
        /// <remarks></remarks>
        public Slot(short ID, byte Count, short Metadata)
        {
            this.ID = ID;
            this.Count = Count;
            this.Metadata = Metadata;
            this.Nbt = new NbtFile();
        }

        public override string ToString()
        {
            return "{ID: " + this.ID + ", Count: " + this.Count + ", Metadata: " + this.Metadata + "}";
        }

        /// <summary>
        /// Reads a slot from the given stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Slot ReadSlot(Stream stream)
        {
            Slot s = new Slot();
            PacketReader pr = new PacketReader(stream);
            s.ID = pr.ReadShort();
            if (s.ID == -1)
            {
                s.Data = pr.Payload;
                return s;
            }
            s.Count = pr.ReadByte();
            s.Metadata = pr.ReadShort();

            if (CanEnchant(s.ID))
            {
                short length = pr.ReadShort();
                if (length != -1)
                {
                    byte[] compressed = new byte[length];
                    compressed = pr.ReadBytes(length);
                    MemoryStream output = new MemoryStream();
                    GZipStream gzs = new GZipStream(new MemoryStream(compressed), CompressionMode.Decompress, false);
                    gzs.CopyTo(output);
                    gzs.Close();
                    s.Nbt = new NbtFile();
                    s.Nbt.LoadFile(output, false);
                }
            }
            s.Data = pr.Payload;
            return s;
        }

        /// <summary>
        /// Gets the slot data.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] GetData()
        {
            byte[] data = new byte[0]
                .Concat(Packet.MakeShort(ID)).ToArray();
            if (ID == -1)
                return data;
            data = data.Concat(new byte[] { Count })
                .Concat(Packet.MakeShort(Metadata)).ToArray();

            if (CanEnchant(ID))
            {
                MemoryStream ms = new MemoryStream();
                GZipStream gzs = new GZipStream(ms, CompressionMode.Compress, false);
                Nbt.SaveFile(gzs);
                gzs.Close();
                byte[] b = ms.GetBuffer();
                data = data.Concat(Packet.MakeShort((short)b.Length)).Concat(b).ToArray();
            }
            return data;
        }

        /// <summary>
        /// Determines whether this instance can enchant the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if this instance can enchant the specified value; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        private static bool CanEnchant(short value)
        {
            return  (256 <= value && value <= 259) ||
                    (267 <= value && value <= 279) ||
                    (283 <= value && value <= 286) ||
                    (290 <= value && value <= 294) ||
                    (298 <= value && value <= 317) ||
                    value == 261 ||
                    value == 346;
        }
    }
}
