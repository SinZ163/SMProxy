using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace LibMinecraft.Model.Packets
{
    /// <summary>
    /// Represents an abstract packet and handles some packet logic.
    /// </summary>
    /// <remarks></remarks>
    public abstract class Packet
    {
        #region Static Helpers

        /// <summary>
        /// Strings the length.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected static int StringLength(string str)
        {
            return 2 + str.Length * 2;
        }

        /// <summary>
        /// Makes the string.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] MakeString(String msg)
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
        public static byte[] MakeInt(int i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        /// <summary>
        /// Makes the absolute int.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] MakeAbsoluteInt(double i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(i * 32.0)));
        }

        /// <summary>
        /// Makes the long.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] MakeLong(long i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        /// <summary>
        /// Makes the short.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] MakeShort(short i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }

        public static byte[] MakeUShort(ushort i)
        {
            return MakeShort((short)i);
        }

        /// <summary>
        /// Makes the double.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] MakeDouble(double d)
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
        public static byte[] MakeFloat(float f)
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
        public static byte MakePackedByte(float f)
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
        public static byte[] MakeBoolean(Boolean b)
        {
            BooleanArray[0] = (byte)(b ? 1 : 0);
            return BooleanArray;
        }

        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int ReadInt(Stream s)
        {
            return IPAddress.HostToNetworkOrder((int)Read(s, 4));
        }

        /// <summary>
        /// Reads the short.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static short ReadShort(Stream s)
        {
            return IPAddress.HostToNetworkOrder((short)Read(s, 2));
        }

        /// <summary>
        /// Reads the long.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static long ReadLong(Stream s)
        {
            return IPAddress.HostToNetworkOrder((long)Read(s, 8));
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static double ReadDouble(Stream s)
        {
            byte[] doubleArray = new byte[sizeof(double)];
            s.Read(doubleArray, 0, sizeof(double));
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
        public static unsafe float ReadFloat(Stream s)
        {
            byte[] floatArray = new byte[sizeof(int)];
            s.Read(floatArray, 0, sizeof(int));
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
        public static Boolean ReadBoolean(Stream s)
        {
            return new BinaryReader(s).ReadBoolean();
        }

        /// <summary>
        /// Reads the bytes.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] ReadBytes(Stream s, int count)
        {
            return new BinaryReader(s).ReadBytes(count);
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static String ReadString(Stream s)
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
            return Encoding.BigEndianUnicode.GetString(b);
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="msg">The MSG.</param>
        /// <remarks></remarks>
        public static void WriteString(Stream s, String msg)
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
        public static void WriteInt(Stream s, int i)
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
        public static void WriteLong(Stream s, long i)
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
        public static void WriteShort(Stream s, short i)
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
        public static void WriteDouble(Stream s, double d)
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
        public static void WriteFloat(Stream s, float f)
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
        public static void WriteBoolean(Stream s, Boolean b)
        {
            new BinaryWriter(s).Write(b);
        }

        /// <summary>
        /// Writes the bytes.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="b">The b.</param>
        /// <remarks></remarks>
        public static void WriteBytes(Stream s, byte[] b)
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
        public static Object Read(Stream s, int num)
        {
            byte[] b = new byte[num];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (byte)s.ReadByte();
            }
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

        #endregion
    }
}
