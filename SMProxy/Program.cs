using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace SMProxy
{
    public static class Program
    {
        public static Socket LocalListener;
        private static IPEndPoint destination;

        static void Main(string[] args)
        {
            LocalListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LocalListener.Bind(new IPEndPoint(IPAddress.Loopback, 25564));
            LocalListener.Listen(10);
            LocalListener.BeginAccept(AcceptConnection, null);
            destination = new IPEndPoint(IPAddress.Loopback, 25565);
            Console.ReadKey(true);
        }

        private static void AcceptConnection(IAsyncResult result)
        {
            var local = LocalListener.EndAccept(result);
            var remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            remote.Connect(destination);
            string fileName = "log-" + DateTime.Now.ToShortTimeString() + local.RemoteEndPoint + ".txt";
            fileName = fileName.Replace(':', '-');
            var proxy = new Proxy(new FileLogProvider(fileName));
            proxy.Start(local, remote);
            LocalListener.BeginAccept(AcceptConnection, null);
        }
    }
}
