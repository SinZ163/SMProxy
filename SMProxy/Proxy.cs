using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Net.Sockets;
using Org.BouncyCastle.Crypto;
using SMProxy.Packets;
using System.Diagnostics;

namespace SMProxy
{
    public class Proxy
    {
        public const int ProtocolVersion = 39;

        private const int BufferSize = 4096 * 64; // TODO: Dynamic buffer resizing
        internal static RSAParameters ServerKey;
        internal static RSACryptoServiceProvider CryptoServiceProvider;

        static Proxy()
        {
            // Generate a global server key
            CryptoServiceProvider = new RSACryptoServiceProvider(1024);
            ServerKey = CryptoServiceProvider.ExportParameters(true);
        }

        public Proxy(ILogProvider logProvider, ProxySettings settings)
        {
            LocalEncryptionEnabled = RemoteEncryptionEnabled = false;
            LocalBuffer = new byte[BufferSize];
            RemoteBuffer = new byte[BufferSize];
            LogProvider = logProvider;
            Settings = settings;
        }

        public ProxySettings Settings;
        public Socket LocalSocket, RemoteSocket;
        public bool LocalEncryptionEnabled, RemoteEncryptionEnabled;
        public BufferedBlockCipher LocalEncrypter, LocalDecrypter;
        public BufferedBlockCipher RemoteEncrypter, RemoteDecrypter;
        public bool Connected { get; set; }
        public ILogProvider LogProvider { get; set; }
        internal byte[] LocalBuffer, RemoteBuffer;
        internal int LocalIndex, RemoteIndex;
        internal byte[] LocalSharedKey, RemoteSharedKey;
        internal byte[] RemoteEncryptionResponse;

        /// <summary>
        /// Starts a proxy connection between the two sockets.
        /// </summary>
        /// <param name="localSocket">The client connection.</param>
        /// <param name="remoteSocket">The server connection.</param>
        public void Start(Socket localSocket, Socket remoteSocket)
        {
            LocalSocket = localSocket;
            RemoteSocket = remoteSocket;
            Connected = true;
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
                    LogProvider.Log("Local client " + LocalSocket.RemoteEndPoint + " disconnected: " + error);
                else
                    LogProvider.Log("Local client " + LocalSocket.RemoteEndPoint + " disconnected.");
                if (RemoteSocket.Connected)
                    RemoteSocket.BeginDisconnect(false, null, null);
                Connected = false;
                if (Settings.SingleSession)
                    Process.GetCurrentProcess().Kill();
                return;
            }
            try
            {
                var packets = PacketReader.TryReadPackets(this, length, PacketContext.ClientToServer);
                foreach (Packet packet in packets)
                {
                    if (packet is InvalidPacket)
                    {
                        LogProvider.Raw(packet.Payload, this, PacketContext.ClientToServer);
                        if (RemoteSocket.Connected && !packet.OverrideSendPacket())
                        {
                            if (RemoteEncryptionEnabled)
                                RemoteSocket.BeginSend(RemoteEncrypter.ProcessBytes(packet.Payload), 0, packet.Payload.Length, SocketFlags.None, null, null);
                            else
                                RemoteSocket.BeginSend(packet.Payload, 0, packet.Payload.Length, SocketFlags.None, null, null);
                        }
                        throw new InvalidOperationException("Unrecognized packet: 0x" + packet.PacketId.ToString("X2"));
                    }
                    packet.HandlePacket(this);
                    LogProvider.Log(packet, this);
                    if (RemoteSocket.Connected && !packet.OverrideSendPacket() && !Settings.ServerSupressedPackets.Contains(packet.PacketId))
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
                LogProvider.Log("Client exception: \"" + e.Message + "\" Switching client to generic TCP proxy");
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
                    LogProvider.Log("Local client " + LocalSocket.RemoteEndPoint + " disconnected: " + error);
                else
                    LogProvider.Log("Local client " + LocalSocket.RemoteEndPoint + " disconnected.");
                if (RemoteSocket.Connected)
                    RemoteSocket.BeginDisconnect(false, null, null);
                Connected = false;
                if (Settings.SingleSession)
                    Process.GetCurrentProcess().Kill();
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
                    LogProvider.Log("Remote server " + RemoteSocket.RemoteEndPoint + " disconnected: " + error);
                else
                    LogProvider.Log("Remote server " + RemoteSocket.RemoteEndPoint + " disconnected.");
                if (LocalSocket.Connected)
                    LocalSocket.BeginDisconnect(false, null, null);
                Connected = false;
                if (Settings.SingleSession)
                    Process.GetCurrentProcess().Kill();
                return;
            }
            try
            {
                var packets = PacketReader.TryReadPackets(this, length, PacketContext.ServerToClient);
                foreach (Packet packet in packets)
                {
                    if (packet is InvalidPacket)
                    {
                        LogProvider.Raw(packet.Payload, this, PacketContext.ServerToClient);
                        if (LocalSocket.Connected && !packet.OverrideSendPacket())
                        {
                            if (LocalEncryptionEnabled)
                                LocalSocket.BeginSend(LocalEncrypter.ProcessBytes(packet.Payload), 0, packet.Payload.Length, SocketFlags.None, null, null);
                            else
                                LocalSocket.BeginSend(packet.Payload, 0, packet.Payload.Length, SocketFlags.None, null, null);
                        }
                        throw new InvalidOperationException("Unrecognized packet: 0x" + packet.PacketId.ToString("X2"));
                    }
                    packet.HandlePacket(this);
                    LogProvider.Log(packet, this);
                    if (LocalSocket.Connected && !packet.OverrideSendPacket() && !Settings.ClientSupressedPackets.Contains(packet.PacketId))
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
                LogProvider.Log("Server exception: \"" + e.Message + "\" Switching server to generic TCP proxy");
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
                    LogProvider.Log("Remote server " + RemoteSocket.RemoteEndPoint + " disconnected: " + error);
                else
                    LogProvider.Log("Remote server " + RemoteSocket.RemoteEndPoint + " disconnected.");
                if (LocalSocket.Connected)
                    LocalSocket.BeginDisconnect(false, null, null);
                Connected = false;
                if (Settings.SingleSession)
                    Process.GetCurrentProcess().Kill();
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
