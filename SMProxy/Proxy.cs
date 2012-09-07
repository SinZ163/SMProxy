using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Net.Sockets;
using Org.BouncyCastle.Crypto;

namespace SMProxy
{
    public class Proxy
    {
        public const int ProtocolVersion = 39;

        private const int BufferSize = 4096 * 64;
        internal static RSAParameters ServerKey;
        internal static RSACryptoServiceProvider CryptoServiceProvider;

        static Proxy()
        {
            // Generate a global server key
            CryptoServiceProvider = new RSACryptoServiceProvider(1024);
            ServerKey = CryptoServiceProvider.ExportParameters(true);
        }

        public Proxy(ILogProvider logProvider)
        {
            LocalEncryptionEnabled = RemoteEncryptionEnabled = false;
            LocalBuffer = new byte[BufferSize];
            RemoteBuffer = new byte[BufferSize];
            IsLocalRaw = IsRemoteRaw = false;
            LogProvider = logProvider;
        }

        public Socket LocalSocket, RemoteSocket;
        public bool LocalEncryptionEnabled, RemoteEncryptionEnabled;
        public BufferedBlockCipher LocalEncrypter, LocalDecrypter;
        public BufferedBlockCipher RemoteEncrypter, RemoteDecrypter;
        public ILogProvider LogProvider { get; set; }
        internal byte[] LocalBuffer, RemoteBuffer;
        internal int LocalIndex, RemoteIndex;
        internal byte[] LocalSharedKey, RemoteSharedKey;
        internal byte[] RemoteEncryptionResponse;

        private bool IsLocalRaw, IsRemoteRaw;

        /// <summary>
        /// Starts a proxy connection between the two sockets.
        /// </summary>
        /// <param name="localSocket">The client connection.</param>
        /// <param name="remoteSocket">The server connection.</param>
        public void Start(Socket localSocket, Socket remoteSocket)
        {
            LocalSocket = localSocket;
            RemoteSocket = remoteSocket;

            LocalSocket.BeginReceive(LocalBuffer, 0, LocalBuffer.Length, SocketFlags.None, HandleLocalPackets, null);
            RemoteSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, HandleRemotePackets, null);
        }

        internal void HandleLocalPackets(IAsyncResult result)
        {
            SocketError error;
            int length = LocalSocket.EndReceive(result, out error) + LocalIndex;
            if (error != SocketError.Success || !LocalSocket.Connected || length == LocalIndex)
            {
                if (error != SocketError.Success)
                {
                    // TODO: Log error
                }
                // Disconnect
                return;
            }
            try
            {
                var packets = PacketReader.TryReadPackets(this, length, PacketContext.ClientToServer);
                foreach (Packet packet in packets)
                {
                    packet.HandlePacket(this);
                    LogProvider.Log(packet, this);
                    if (RemoteSocket.Connected && !packet.OverrideSendPacket())
                    {
                        if (RemoteEncryptionEnabled)
                            RemoteSocket.BeginSend(RemoteEncrypter.ProcessBytes(packet.Payload), 0, packet.Payload.Length, SocketFlags.None, null, null);
                        else
                            RemoteSocket.BeginSend(packet.Payload, 0, packet.Payload.Length, SocketFlags.None, null, null);
                    }
                }
                LocalSocket.BeginReceive(LocalBuffer, LocalIndex, LocalBuffer.Length - LocalIndex, SocketFlags.None, HandleLocalPackets, null);
            }
            catch (Exception e)
            {
                IsLocalRaw = true;
                byte[] payload = new byte[length];
                Array.Copy(LocalBuffer, LocalIndex, payload, 0, payload.Length);

                LogProvider.Log("Client exception: \"" + e.Message + "\" Switching client to generic TCP proxy");
                LogProvider.Raw(payload, this, PacketContext.ClientToServer);

                if (RemoteEncryptionEnabled)
                    payload = RemoteEncrypter.ProcessBytes(payload);

                RemoteSocket.BeginSend(payload, 0, payload.Length, SocketFlags.None, null, null);
                LocalSocket.BeginReceive(LocalBuffer, 0, LocalBuffer.Length, SocketFlags.None, HandleLocalRaw, null);
            }
        }

        private void HandleLocalRaw(IAsyncResult result)
        {
            SocketError error;
            int length = LocalSocket.EndReceive(result, out error);
            if (error != SocketError.Success || !LocalSocket.Connected || length == 0)
            {
                if (error != SocketError.Success)
                {
                    // TODO: Log error
                    Console.WriteLine("Local error: " + error.ToString());
                }
                // Disconnect
                return;
            }

            byte[] payload = new byte[length];
            Array.Copy(LocalBuffer, 0, payload, 0, length);

            if (LocalEncryptionEnabled)
                payload = LocalDecrypter.ProcessBytes(payload);

            LogProvider.Raw(payload, this, PacketContext.ClientToServer);

            if (RemoteEncryptionEnabled)
                payload = RemoteEncrypter.ProcessBytes(payload);

            if (RemoteSocket.Connected)
                RemoteSocket.BeginSend(payload, 0, payload.Length, SocketFlags.None, null, null);
            if (LocalSocket.Connected)
                LocalSocket.BeginReceive(LocalBuffer, 0, LocalBuffer.Length, SocketFlags.None, HandleLocalRaw, null);
        }

        internal void HandleRemotePackets(IAsyncResult result)
        {
            SocketError error;
            int length = RemoteSocket.EndReceive(result, out error) + RemoteIndex;
            if (error != SocketError.Success || !RemoteSocket.Connected || length == RemoteIndex)
            {
                if (error != SocketError.Success)
                {
                    // TODO: Log error
                }
                // Disconnect
                return;
            }
            try
            {
                var packets = PacketReader.TryReadPackets(this, length, PacketContext.ServerToClient);
                foreach (Packet packet in packets)
                {
                    packet.HandlePacket(this);
                    LogProvider.Log(packet, this);
                    if (LocalSocket.Connected && !packet.OverrideSendPacket())
                    {
                        if (LocalEncryptionEnabled)
                            LocalSocket.BeginSend(LocalEncrypter.ProcessBytes(packet.Payload), 0, packet.Payload.Length, SocketFlags.None, null, null);
                        else
                            LocalSocket.BeginSend(packet.Payload, 0, packet.Payload.Length, SocketFlags.None, null, null);
                    }
                }
                RemoteSocket.BeginReceive(RemoteBuffer, RemoteIndex, RemoteBuffer.Length - RemoteIndex, SocketFlags.None, HandleRemotePackets, null);
            }
            catch (Exception e)
            {
                IsRemoteRaw = true;
                byte[] payload = new byte[length];
                Array.Copy(RemoteBuffer, RemoteIndex, payload, 0, payload.Length);

                LogProvider.Log("Server exception: \"" + e.Message + "\" Switching server to generic TCP proxy");
                LogProvider.Raw(payload, this, PacketContext.ServerToClient);

                if (LocalEncryptionEnabled)
                    payload = LocalEncrypter.ProcessBytes(payload);

                LocalSocket.BeginSend(payload, 0, payload.Length, SocketFlags.None, null, null);
                RemoteSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, HandleRemoteRaw, null);
            }
        }

        private void HandleRemoteRaw(IAsyncResult result)
        {
            SocketError error;
            int length = RemoteSocket.EndReceive(result, out error);
            if (error != SocketError.Success || !RemoteSocket.Connected || length == 0)
            {
                if (error != SocketError.Success)
                {
                    // TODO: Log error
                    Console.WriteLine("Remote error: " + error.ToString());
                }
                // Disconnect
                return;
            }

            byte[] payload = new byte[length];
            Array.Copy(RemoteBuffer, 0, payload, 0, length);

            if (RemoteEncryptionEnabled)
                payload = RemoteDecrypter.ProcessBytes(payload);

            LogProvider.Raw(payload, this, PacketContext.ServerToClient);

            if (LocalEncryptionEnabled)
                payload = LocalEncrypter.ProcessBytes(payload);

            if (LocalSocket.Connected)
                LocalSocket.BeginSend(payload, 0, payload.Length, SocketFlags.None, null, null);
            if (RemoteSocket.Connected)
                RemoteSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, HandleRemoteRaw, null);
        }
    }
}
