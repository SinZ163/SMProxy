using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Net;
using System.Diagnostics;
using System.Xml.Linq;
using SevenZip.Compression.LZMA;

namespace SharpLauncher
{
    public static class Minecraft
    {
        private const string LoginUrl = "https://login.minecraft.net?user={0}&password={1}&version=13";
        private const string ResourceUrl = "http://s3.amazonaws.com/MinecraftResources/";
        private const string DownloadUrl = "http://s3.amazonaws.com/MinecraftDownload/";

        public static Session DoLogin(string Username, string Password)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(string.Format(LoginUrl,
                Uri.EscapeUriString(Username),
                Uri.EscapeUriString(Password)));
            var response = request.GetResponse();
            StreamReader responseStream = new StreamReader(response.GetResponseStream());
            string login = responseStream.ReadToEnd();
            responseStream.Close();
            if (login.Count(c => c == ':') != 4)
                return new Session(login.Trim());
            string[] parts = login.Split(':');
            return new Session(parts[2], parts[3], parts[0]);
        }

        public static string GetJavaPath()
        {
            RuntimeInfo.GatherInfo();
            if (RuntimeInfo.IsWindows)
            {
                if (File.Exists("C:\\Program Files\\Java\\jre7\\bin\\java.exe"))
                    return "C:\\Program Files\\Java\\jre7\\bin\\java.exe";
                else
                    return "C:\\Program Files (x86)\\Java\\jre7\\bin\\java.exe";
            }
            else
            {
                if (File.Exists("/usr/bin/java"))
                    return "/usr/bin/java";
                else
                    return "/bin/java";
            }
        }

        public static void LaunchMinecraft(Session session)
        {
            ProcessStartInfo psi = new ProcessStartInfo(GetJavaPath(),
                "-Xms512m -Xmx1g -Djava.library.path=natives/ -cp \"minecraft.jar;lwjgl.jar;lwjgl_util.jar\" net.minecraft.client.Minecraft " +
                session.Username + " " + session.SessionID);
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                ".minecraft/bin");
            Process.Start(psi);
        }

        public static void LaunchMinecraftDemo()
        {
            ProcessStartInfo psi = new ProcessStartInfo(GetJavaPath(),
                "-Xms512m -Xmx1g -Djava.library.path=natives/ -cp \"minecraft.jar;lwjgl.jar;lwjgl_util.jar\" net.minecraft.client.Minecraft Player - -demo");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                ".minecraft/bin");
            Process.Start(psi);
        }

        public static List<FileDownload> GetDownloadLinks()
        {
            List<FileDownload> files = new List<FileDownload>();
            files.AddRange(new FileDownload[]
                {
                    new FileDownload(new Uri(DownloadUrl + "lwjgl.jar"), "bin/lwjgl.jar"),
                    new FileDownload(new Uri(DownloadUrl + "jinput.jar"), "bin/jinput.jar"),
                    new FileDownload(new Uri(DownloadUrl + "lwjgl_util.jar"), "bin/lwjgl_util.jar"),
                    new FileDownload(new Uri(DownloadUrl + "minecraft.jar"), "bin/minecraft.jar")
                });
            string natives = GetNativeArchiveFile();
            files.Add(new FileDownload(new Uri(DownloadUrl + natives), "bin/" + natives));
            return files;
        }

        public static string LastLoginFile
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft/lastlogin");
            }
        }

        public static string DotMinecraft
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
            }
        }

        private static readonly byte[] LastLoginSalt = new byte[] { 0x0c, 0x9d, 0x4a, 0xe4, 0x1e, 0x83, 0x15, 0xfc };
        private const string LastLoginPassword = "passwordfile";
        public static LastLogin GetLastLogin()
        {
            try
            {
                byte[] encryptedLogin = File.ReadAllBytes(LastLoginFile);
                PKCSKeyGenerator crypto = new PKCSKeyGenerator(LastLoginPassword, LastLoginSalt, 5, 1);
                ICryptoTransform cryptoTransform = crypto.Decryptor;
                byte[] decrypted = cryptoTransform.TransformFinalBlock(encryptedLogin, 0, encryptedLogin.Length);
                short userLength = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(decrypted, 0));
                byte[] user = decrypted.Skip(2).Take(userLength).ToArray();
                short passLength = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(decrypted, userLength + 2));
                byte[] password = decrypted.Skip(4 + userLength).ToArray();
                LastLogin result = new LastLogin();
                result.Username = System.Text.Encoding.UTF8.GetString(user);
                result.Password = System.Text.Encoding.UTF8.GetString(password);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private static string GetNativeArchiveFile()
        {
            RuntimeInfo.GatherInfo();
            string natives = "";
            if (RuntimeInfo.IsWindows)
                natives = "windows";
            else if (RuntimeInfo.IsLinux)
                natives = "linux";
            else if (RuntimeInfo.IsMacOSX)
                natives = "macosx";
            else if (RuntimeInfo.IsSolaris)
                natives = "solaris";
            natives += "_natives.jar.lzma";
            return natives;
        }

        public static void SetLastLogin(LastLogin login)
        {
            byte[] decrypted = BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)login.Username.Length))
                .Concat(System.Text.Encoding.UTF8.GetBytes(login.Username))
                .Concat(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)login.Password.Length)))
                .Concat(System.Text.Encoding.UTF8.GetBytes(login.Password)).ToArray();

            PKCSKeyGenerator crypto = new PKCSKeyGenerator(LastLoginPassword, LastLoginSalt, 5, 1);
            ICryptoTransform cryptoTransform = crypto.Encryptor;
            byte[] encrypted = cryptoTransform.TransformFinalBlock(decrypted, 0, decrypted.Length);
            if (File.Exists(LastLoginFile))
                File.Delete(LastLoginFile);
            using (Stream stream = File.Create(LastLoginFile))
                stream.Write(encrypted, 0, encrypted.Length);
        }
    }

    public class LastLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Session
    {
        public Session(string Error)
        {
            this.Error = Error;
        }

        public Session(string Username, string SessionID)
        {
            this.Username = Username;
            this.SessionID = SessionID;
        }

        public Session(string Username, string SessionID, string Version)
            : this(Username, SessionID)
        {
            this.Version = Version;
        }

        public string Username { get; set; }
        public string SessionID { get; set; }
        public string Version { get; set; }
        public string Error { get; set; }
    }

    public class FileDownload
    {
        public FileDownload()
        {
        }

        public FileDownload(Uri DownloadUri, string Destination)
        {
            this.DownloadUri = DownloadUri;
            this.Destination = Destination;
        }

        public Uri DownloadUri { get; set; }
        public string Destination { get; set; }
        public int Size { get; set; }
    }
}
