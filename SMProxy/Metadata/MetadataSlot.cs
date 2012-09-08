﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMProxy;

namespace Craft.Net.Data.Metadata
{
    public class MetadataSlot : MetadataEntry
    {
        public override byte Identifier { get { return 2; } }
        public override string FriendlyName { get { return "float"; } }

        public Slot Value;

        public MetadataSlot(byte index) : base(index)
        {
        }

        public MetadataSlot(byte index, Slot value) : base(index)
        {
            Value = value;
        }

        public override bool TryReadEntry(byte[] buffer, ref int offset)
        {
            if (buffer.Length - offset < 6)
                return false;
            offset++;
            ushort id, metadata;
            byte count;
            if (!DataUtility.TryReadUInt16(buffer, ref offset, out id))
                return false;
            if (!DataUtility.TryReadByte(buffer, ref offset, out count))
                return false;
            if (!DataUtility.TryReadUInt16(buffer, ref offset, out metadata))
                return false;
            Value = new Slot(id, count, metadata);
            return true;
        }

        public override byte[] Encode()
        {
            byte[] data = new byte[]
                              {
                                  GetKey(),
                                  0, 0,
                                  Value.Count,
                                  0, 0
                              };
            Array.Copy(DataUtility.CreateUInt16(Value.Id), 0, data, 1, 2);
            Array.Copy(DataUtility.CreateUInt16(Value.Metadata), 0, data, 4, 2);
            return data;
        }
    }
}
