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
        public override string ToString()
        {
            Type type = GetType();
            string value = "";
            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
            {
                value += "    " + field.Name + ": " + field.GetValue(this) + "\n";
            }
            return value.Remove(value.Length - 1);
        }
    }
}
