using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using SharpLauncher;

namespace SMProxy
{
    public static class Program
    {
        public static Socket LocalListener;
        private static ProxySettings settings;

        static void Main(string[] args)
        {
            settings = new ProxySettings();
            bool setRemote = false;
            string username = null, password = null;
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "--username":
                    case "-u":
                        username = args[++i];
                        break;
                    case "--password":
                    case "-p":
                        password = args[++i];
                        break;
                    case "--suppress-client":
                    case "-sc":
                        settings.LogClient = false;
                        break;
                    case "--suppress-server":
                    case "-ss":
                        settings.LogServer = false;
                        break;
                    case "--enable-profiling":
                    case "-pr":
                        settings.EnableProfiling = true; // TODO
                        break;
                    case "--filter":
                    case "-f":
                        string filter = args[++i];
                        string[] ids = filter.Split(',');
                        settings.FilterPackets = new byte[ids.Length];
                        int j = 0;
                        foreach (var id in ids)
                            settings.FilterPackets[j++] = byte.Parse(id, NumberStyles.HexNumber);
                        break;
                    case "--!filter":
                    case "-!f":
                        string exclude = args[++i];
                        string[] xids = exclude.Split(',');
                        settings.UnloggedPackets = new byte[xids.Length];
                        int k = 0;
                        foreach (var id in xids)
                            settings.UnloggedPackets[k++] = byte.Parse(id, NumberStyles.HexNumber);
                        break;
                    case "--endpoint":
                    case "-ep":
                        settings.LocalEndPoint = ParseEndPoint(args[++i]);
                        break;
                    case "--suppress-packet":
                    case "-sp":
                        string packet = args[++i].ToLower();
                        string[] parts = packet.Split(':');
                        if (parts[1].Contains("c"))
                            settings.ClientSupressedPackets.Add(byte.Parse(parts[0], NumberStyles.HexNumber));
                        if (parts[1].Contains("s"))
                            settings.ServerSupressedPackets.Add(byte.Parse(parts[0], NumberStyles.HexNumber));
                        break;
                    case "--persistent-session":
                    case "-ps":
                        settings.SingleSession = false;
                        break;
                    case "--output":
                    case "-o":
                        settings.FileName = args[++i];
                        break;
                    case "--port":
                        settings.LocalEndPoint.Port = int.Parse(args[++i]);
                        break;
                    default:
                        if (!setRemote)
                        {
                            settings.RemoteEndPoint = ParseEndPoint(arg);
                            setRemote = true;
                        }
                        else
                        {
                            DisplayHelp();
                            return;
                        }
                        break;
                }
            }

            if (username == null)
            {
                var login = Minecraft.GetLastLogin();
                if (login != null)
                {
                    username = login.Username;
                    password = login.Password;
                }
            }
            else if (password == null)
            {
                Console.Write("Password for " + username + ": ");
                password = ReadPassword();
            }

            if (username != null && password != null)
            {
                var session = Minecraft.DoLogin(username, password);
                if (string.IsNullOrEmpty(session.Error))
                    settings.UserSession = session.SessionID;
                else
                {
                    Console.WriteLine("Login failed.");
                    return;
                }
            }

            LocalListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LocalListener.Bind(settings.LocalEndPoint);
            LocalListener.Listen(10);
            LocalListener.BeginAccept(AcceptConnection, null);

            Console.WriteLine("Listening on " + settings.LocalEndPoint + "; Press any key to exit.");

            Console.ReadKey(true);
        }

        private static void AcceptConnection(IAsyncResult result)
        {
            var local = LocalListener.EndAccept(result);
            Console.WriteLine("Recieved connection from " + local.RemoteEndPoint + ", proxying to " + settings.RemoteEndPoint);
            var remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            remote.Connect(settings.RemoteEndPoint);

            var proxy = new Proxy(new FileLogProvider(settings, GetFileName((IPEndPoint)local.RemoteEndPoint)), settings);
            proxy.Start(local, remote);
            if (!settings.SingleSession)
                LocalListener.BeginAccept(AcceptConnection, null);
        }

        private static string GetFileName(IPEndPoint endPoint)
        {
            if (settings.FileName != null)
                return settings.FileName;
            var time = DateTime.Now;
            return "log_" + time.Hour + "-" + time.Minute + "-" + time.Second + "_" + endPoint.Address + ".txt";
        }

        private static IPEndPoint ParseEndPoint(string arg)
        {
            string[] parts = arg.Split(':');
            IPAddress address;
            if (!IPAddress.TryParse(parts[0], out address))
                address = Dns.GetHostEntry(parts[0]).AddressList.First(i => i.AddressFamily == AddressFamily.InterNetwork);
            if (parts.Length == 1)
                return new IPEndPoint(address, 25565);
            return new IPEndPoint(address, int.Parse(parts[1]));
        }

        public static string ReadPassword(char mask)
        {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ }; // const

            var pass = new Stack<char>();
            char chr;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (chr == BACKSP)
                {
                    if (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (chr == CTRLBACKSP)
                {
                    while (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (FILTERED.Count(x => chr == x) > 0) { }
                else
                {
                    pass.Push((char)chr);
                    Console.Write(mask);
                }
            }

            Console.WriteLine();

            return new string(pass.Reverse().ToArray());
        }

        /// <summary>
        /// Like System.Console.ReadLine(), only with a mask.
        /// </summary>
        /// <returns>the string the user typed in </returns>
        public static string ReadPassword()
        {
            return ReadPassword('*');
        }


        private static void DisplayHelp()
        {
            Console.WriteLine("Usage:");
        }
    }
}
