using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SMProxy
{
    static partial class Program
    {
        static void LogPacket(StreamWriter sw, bool ClientToServer, byte PacketID, PacketReader pr, params object[] args)
        {
            try
            {
                if (sw == null)
                    return;
                bool Suppressed = (ClientToServer && ClientDenyPackets.Contains(PacketID)) ||
                                  (!ClientToServer && ServerDenyPackets.Contains(PacketID));
                if (FilterOutput && !Filter.Contains(PacketID))
                    return;
                if (IgnoreFilterOutput && IgnoreFilter.Contains(PacketID))
                    return;
                if (ClientToServer && SuppressClient)
                    return;
                if (!ClientToServer && SuppressServer)
                    return;
                if (ClientToServer)
                    sw.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [CLIENT->SERVER" + (Suppressed ? " SUPPRESSED" : "") + "]: " +
                        ((LibMinecraft.Model.PacketID)PacketID).ToString() + " (0x" + PacketID.ToString("x") + ")");
                else
                    sw.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [SERVER->CLIENT" + (Suppressed ? " SUPPRESSED" : "") + "]: " +
                        ((LibMinecraft.Model.PacketID)PacketID).ToString() + " (0x" + PacketID.ToString("x") + ")");
                if (pr.Payload.Length == 0)
                    return;
                sw.WriteLine("\t[" + DumpArray(pr.Payload) + "]");
                for (int i = 0; i < args.Length; i += 2)
                {
                    if (args[i + 1] is byte[])
                        sw.WriteLine("\t" + args[i].ToString() + " (Byte[]): [" + DumpArray((byte[])args[i + 1]) + "]");
                    else
                        sw.WriteLine("\t" + args[i].ToString() + " (" + args[i + 1].GetType().Name + "): " + args[i + 1]);
                }
                sw.Flush();
            }
            catch { }
        }

        static void LogPacket(StreamWriter sw, bool ClientToServer, byte PacketID, string name, PacketReader pr, params object[] args)
        {
            try
            {
                if (sw == null)
                    return;
                bool Suppressed = (ClientToServer && ClientDenyPackets.Contains(PacketID)) ||
                                  (!ClientToServer && ServerDenyPackets.Contains(PacketID));
                if (FilterOutput && !Filter.Contains(PacketID))
                    return;
                if (ClientToServer && SuppressClient)
                    return;
                if (!ClientToServer && SuppressServer)
                    return;
                if (ClientToServer)
                    sw.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [CUSTOM CLIENT->SERVER" + (Suppressed ? " SUPPRESSED" : "") + "]: " +
                        name + " (0x" + PacketID.ToString("x") + ")");
                else
                    sw.WriteLine("{" + DateTime.Now.ToLongTimeString() + "} [CUSTOM SERVER->CLIENT" + (Suppressed ? " SUPPRESSED" : "") + "]: " +
                        name + " (0x" + PacketID.ToString("x") + ")");
                if (pr.Payload.Length == 0)
                    return;
                sw.WriteLine("\t[" + DumpArray(pr.Payload) + "]");
                for (int i = 0; i < args.Length; i += 2)
                {
                    if (args[i + 1] is byte[])
                        sw.WriteLine("\t" + args[i].ToString() + " (Byte[]): [" + DumpArray((byte[])args[i + 1]) + "]");
                    else
                        sw.WriteLine("\t" + args[i].ToString() + " (" + args[i + 1].GetType().Name + "): " + args[i + 1]);
                }
                sw.Flush();
            }
            catch { }
        }

        static void LogProfiling(StreamWriter sw, DateTime downloadStartTime, DateTime downloadCompleteTime,
            DateTime uploadStartTime, bool ClientToServer, byte Packet, PacketReader pr)
        {
            if (!EnableProfiling)
                return;
            if (ClientToServer && SuppressClient)
                return;
            if (!ClientToServer && SuppressServer)
                return;
            if (FilterOutput && !Filter.Contains(Packet))
                return;
            DateTime uploadCompleteTime = DateTime.Now;
            string output = "\tProfiling: Size: " + pr.Payload.Length + "; down: " +
                (downloadCompleteTime - downloadStartTime).TotalMilliseconds + " ms (" +
                (pr.Payload.Length / (downloadCompleteTime - downloadStartTime).TotalSeconds) + " bytes/sec); up: " +
                (uploadCompleteTime - uploadStartTime).TotalMilliseconds + " ms (" +
                (pr.Payload.Length / (uploadCompleteTime - uploadStartTime).TotalSeconds) + " bytes/sec); Proxy lag: " +
                ((uploadCompleteTime - downloadStartTime) - (downloadStartTime - downloadCompleteTime)).TotalMilliseconds + " ms";
            sw.WriteLine(output);
            sw.Flush();
        }

        static string DumpArray(byte[] resp)
        {
            string res = "";
            foreach (byte b in resp)
                res += b.ToString("x2") + ":";
            return res.Remove(res.Length - 1);
        }

        public static long CopyTo(this Stream source, Stream destination)
        {
            byte[] buffer = new byte[2048];
            int bytesRead;
            long totalBytes = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
            }
            return totalBytes;
        }
    }
}
