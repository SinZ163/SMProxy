using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy
{
    public interface ILogProvider
    {
        void Log(string text);
        void Log(Packet packet, Proxy proxy);
        void Raw(byte[] payload, Proxy proxy, PacketContext context);
    }
}
