using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibMinecraft.Model;

namespace SMProxy
{
    class PacketReader
    {
        public byte[] Payload { get; set; }
        public TcpClient Client { get; set; }
        private Stream s { get; set; }

        public PacketReader(TcpClient Client)
        {
            Payload = new byte[0];
            this.Client = Client;
            this.s = Client.GetStream();
        }

        public PacketReader(Stream s)
        {
            Payload = new byte[0];
            this.s = s;
        }

        /// <summary>
        /// Strings the length.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected int StringLength(string str)
        {
            return 2 + str.Length * 2;
        }

        /// <summary>
        /// Makes the string.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeString(String msg)
        {
            short len = IPAddress.HostToNetworkOrder((short)msg.Length);
            byte[] a = BitConverter.GetBytes(len);
            byte[] b = Encoding.BigEndianUnicode.GetBytes(msg);
            return a.Concat(b).ToArray();
        }

        /// <summary>
        /// Makes the int.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeInt(int i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        /// <summary>
        /// Makes the absolute int.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeAbsoluteInt(double i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(i * 32.0)));
        }

        /// <summary>
        /// Makes the long.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeLong(long i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        /// <summary>
        /// Makes the short.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeShort(short i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        public byte[] MakeUShort(ushort i)
        {
            return MakeShort((short)i);
        }

        /// <summary>
        /// Makes the double.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeDouble(double d)
        {
            byte[] b = BitConverter.GetBytes(d);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return b;
        }

        /// <summary>
        /// Makes the float.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeFloat(float f)
        {
            byte[] b = BitConverter.GetBytes(f);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return b;
        }

        /// <summary>
        /// Makes the packed byte.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte MakePackedByte(float f)
        {
            return (byte)(((Math.Floor(f) % 360) / 360) * 256);
        }

        /// <summary>
        /// 
        /// </summary>
        static byte[] BooleanArray = new byte[] { 0 };
        /// <summary>
        /// Makes the boolean.
        /// </summary>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] MakeBoolean(Boolean b)
        {
            BooleanArray[0] = (byte)(b ? 1 : 0);
            return BooleanArray;
        }

        public Slot ReadSlot()
        {
            Slot slot = Slot.ReadSlot(s);
            Payload = Payload.Concat(slot.Data).ToArray();
            return slot;
        }

        public byte ReadByte()
        {
            byte b = (byte)s.ReadByte();
            Payload = Payload.Concat(new byte[] { b }).ToArray();
            return b;
        }

        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public int ReadInt()
        {
            return IPAddress.HostToNetworkOrder((int)Read(4));
        }

        /// <summary>
        /// Reads the int from the stream, but ignores the value.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public int ReadInt(int value)
        {
            byte[] b = new byte[4];
            s.Read(b, 0, 4);
            int i = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(b, 0));
            Payload = Payload.Concat(MakeInt(value)).ToArray();
            return value;
        }

        /// <summary>
        /// Reads the short.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public short ReadShort()
        {
            return IPAddress.HostToNetworkOrder((short)Read(2));
        }

        /// <summary>
        /// Reads the long.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public long ReadLong()
        {
            return IPAddress.HostToNetworkOrder((long)Read(8));
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public double ReadDouble()
        {
            byte[] doubleArray = new byte[sizeof(double)];
            s.Read(doubleArray, 0, sizeof(double));
            Payload = Payload.Concat(doubleArray).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(doubleArray);
            return BitConverter.ToDouble(doubleArray, 0);
        }

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public unsafe float ReadFloat()
        {
            byte[] floatArray = new byte[sizeof(int)];
            s.Read(floatArray, 0, sizeof(int));
            Payload = Payload.Concat(floatArray).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(floatArray);
            int i = BitConverter.ToInt32(floatArray, 0);
            return *(float*)&i;
        }

        /// <summary>
        /// Reads the boolean.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Boolean ReadBoolean()
        {
            return ReadByte() == 1;
        }

        /// <summary>
        /// Reads the bytes.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public byte[] ReadBytes(int count)
        {
            byte[] b = new BinaryReader(s).ReadBytes(count);
            Payload = Payload.Concat(b).ToArray();
            return b;
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public String ReadString()
        {
            short len;
            byte[] a = new byte[2];
            a[0] = (byte)s.ReadByte();
            a[1] = (byte)s.ReadByte();
            len = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(a, 0));
            byte[] b = new byte[len * 2];
            for (int i = 0; i < len * 2; i++)
            {
                b[i] = (byte)s.ReadByte();
            }
            Payload = Payload.Concat(a.Concat(b)).ToArray();
            return Encoding.BigEndianUnicode.GetString(b);
        }

        public byte[] ReadMobMetadata()
        {
            byte[] b = new byte[0];
            byte value = 0;
            while (value != 127)
            {
                value = ReadByte();
                b = b.Concat(new byte[] { value }).ToArray();
                if (value != 127)
                {
                    switch (value >> 5)
                    {
                        case 0:
                            b = b.Concat(ReadBytes(1)).ToArray();
                            break;
                        case 1:
                            b = b.Concat(ReadBytes(2)).ToArray();
                            break;
                        case 2:
                        case 3:
                            b = b.Concat(ReadBytes(4)).ToArray();
                            break;
                        case 4:
                            b = b.Concat(ReadBytes(16)).ToArray();
                            break;
                        case 5:
                            b = b.Concat(ReadBytes(5)).ToArray();
                            break;
                        case 6:
                            b = b.Concat(ReadBytes(12)).ToArray();
                            break;
                    }
                }
            }
            return b;
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="msg">The MSG.</param>
        /// <remarks></remarks>
        public void WriteString(String msg)
        {

            short len = IPAddress.HostToNetworkOrder((short)msg.Length);
            byte[] a = BitConverter.GetBytes(len);
            byte[] b = Encoding.BigEndianUnicode.GetBytes(msg);
            byte[] c = a.Concat(b).ToArray();
            s.Write(c, 0, c.Length);
        }

        /// <summary>
        /// Writes the int.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="i">The i.</param>
        /// <remarks></remarks>
        public void WriteInt(int i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            s.Write(a, 0, a.Length);
        }

        /// <summary>
        /// Writes the long.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="i">The i.</param>
        /// <remarks></remarks>
        public void WriteLong(long i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            s.Write(a, 0, a.Length);
        }

        /// <summary>
        /// Writes the short.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="i">The i.</param>
        /// <remarks></remarks>
        public void WriteShort(short i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            s.Write(a, 0, a.Length);
        }

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="d">The d.</param>
        /// <remarks></remarks>
        public void WriteDouble(double d)
        {
            byte[] doubleArray = BitConverter.GetBytes(d);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(doubleArray);
            s.Write(doubleArray, 0, sizeof(double));
        }

        /// <summary>
        /// Writes the float.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="f">The f.</param>
        /// <remarks></remarks>
        public void WriteFloat(float f)
        {
            byte[] floatArray = BitConverter.GetBytes(f);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(floatArray);
            s.Write(floatArray, 0, sizeof(float));
        }

        /// <summary>
        /// Writes the boolean.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <remarks></remarks>
        public void WriteBoolean(Boolean b)
        {
            new BinaryWriter(s).Write(b);
        }

        /// <summary>
        /// Writes the bytes.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="b">The b.</param>
        /// <remarks></remarks>
        public void WriteBytes(byte[] b)
        {
            new BinaryWriter(s).Write(b);
        }

        /// <summary>
        /// Reads the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Object Read(int num)
        {
            byte[] b = new byte[num];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (byte)s.ReadByte();
            }
            Payload = Payload.Concat(b).ToArray();
            switch (num)
            {
                case 4:
                    return BitConverter.ToInt32(b, 0);
                case 8:
                    return BitConverter.ToInt64(b, 0);
                case 2:
                    return BitConverter.ToInt16(b, 0);
                default:
                    return 0;
            }
        }
    }
}
