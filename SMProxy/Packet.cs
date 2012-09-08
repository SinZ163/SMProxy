using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SMProxy
{
    public enum PacketContext
    {
        ClientToServer,
        ServerToClient
    }

    public abstract class Packet
    {
        /// <summary>
        /// The direction this packet travels.
        /// </summary>
        public PacketContext PacketContext { get; set; }
        /// <summary>
        /// This packet's packet ID.
        /// </summary>
        public abstract byte PacketId { get; }

        public byte[] Payload { get; set; }

        /// <summary>
        /// This event fires after the packet has been sent.
        /// </summary>
        public event EventHandler OnPacketSent;

        internal void FirePacketSent()
        {
            if (OnPacketSent != null)
                OnPacketSent(this, null);
        }

        /// <summary>
        /// Attempts to read a packet from the given buffer. Returns the length of
        /// the packet if successful, or -1 if the packet is incomplete.
        /// </summary>
        public abstract int TryReadPacket(byte[] buffer, int length);

        /// <summary>
        /// Handles additional logic for recieving the packet.
        /// </summary>
        public virtual void HandlePacket(Proxy proxy)
        {
        }

        public virtual bool OverrideSendPacket()
        {
            return false;
        }

        /// <summary>
        /// Converts the packet to a human-readable format.
        /// </summary>
        public string ToString(Proxy proxy)
        {
            Type type = GetType();
            string value = "";
            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
            {
                var nameAttribute = Attribute.GetCustomAttribute(field, typeof(FriendlyNameAttribute)) as FriendlyNameAttribute;
                var name = field.Name;

                if (nameAttribute != null)
                    name = nameAttribute.FriendlyName;
                else
                    name = AddSpaces(name);

                value += "    " + name + " (" + field.FieldType.Name + ")";

                var fValue = field.GetValue(this);
                string fieldValue = fValue.ToString();
                if (fValue is byte[])
                    fieldValue = DataUtility.DumpArray(fValue as byte[]);
                value += ": " + fieldValue + "\n";
            }
            return value.Remove(value.Length - 1);
        }

        public static string AddSpaces(string value)
        {
            string newValue = "";
            foreach (char c in value)
            {
                if (char.IsLower(c))
                    newValue += c;
                else
                    newValue += " " + c;
            }
            return newValue.Substring(1);
        }
    }
}
