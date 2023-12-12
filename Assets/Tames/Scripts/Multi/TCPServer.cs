using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Collections;

namespace Multi
{


    public class TCPServer : MonoBehaviour
    {
        const byte RegisterRequest = 12;
        const byte EmailRequest = 14;
        public string address;
        public string mainPort;
        public string altPort;
        public string email;
        public string password;
        public string authenticator;
        public static string Address = "";
        public static string MainPort = "";
        public static string AltPort = "";
        public static string Email = "";
        public static string Password = "";
        public static string Authenticator = "";
        public static CoreTame core;
        public static TCPServer Singleton;
        public static bool serverSuccess = false;
        public static TcpListener Server = null;
        private void Start()
        {
            CoreTame.applicationPath = Application.dataPath;
            CoreTame.applicationPath = CoreTame.applicationPath.Substring(0, CoreTame.applicationPath.Length - 1);
            int slash = Mathf.Max(CoreTame.applicationPath.LastIndexOf("/"), CoreTame.applicationPath.LastIndexOf('\\'));
            CoreTame.applicationPath = CoreTame.applicationPath.Substring(0, slash + 1);
            LoadSettings();
            Singleton = this;
            StartServer();
        }
        void LoadSettings()
        {
            string[] lines = File.ReadAllLines(CoreTame.applicationPath + "settings.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string[] s = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (s.Length > 1)
                {
                    switch (s[0])
                    {
                        case "ip": Address = s[1]; break;
                        case "main-port": MainPort = s[1]; break;
                        case "2nd-port": AltPort = s[1]; break;
                        case "email": Email = s[1]; break;
                        case "email-password": Password = s[1]; break;
                        case "server-password": Authenticator = s[1]; break;
                    }
                }
                if (Address == "") Address = address;
                if (MainPort == "") MainPort = mainPort;
                if (AltPort == "") AltPort = altPort;
                if (Email == "") Email = email;
                if (Password == "") Password = password;
                if (Authenticator == "") Authenticator = authenticator;
            }
        }
        public static void StartServer()
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(Address);
                Server = new TcpListener(localAddr, int.Parse(MainPort));

                // Start listening for client requests.
                Server.Start();
                serverSuccess = true;

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            if (serverSuccess)
                Singleton.ResumeServer();
        }
        async public void ResumeServer()
        {
            List<NetworkStream> streams = new List<NetworkStream>();
            Debug.Log("resuming ");
            while (true)
            {
                await Task.Run(() =>
                {
                    TcpClient client = Server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    if (stream != null)
                    {
                        //  MemoryStream ms = new MemoryStream();
                        if (stream.CanRead)

                            ProcessStream(stream);


                    }
                    client.Close();
                });
            }
        }

        public void ProcessStream(NetworkStream stream)
        {
            Debug.Log("received ...");
            byte[] b = new byte[3];
            stream.Read(b, 0, 3);
            int n = b[1] * 256 + b[2];
            switch (b[0])
            {
                case RegisterRequest: GetRegister(stream, n); break;
                case EmailRequest: GetEmail(stream, n); break;
            }
        }
        string ByteToString(byte[] b, int n)
        {
            string s = "";
            char c;
            for (int i = 0; i < b.Length; i += 2)
            {
                ushort u = (ushort)(b[i] * 256 + b[i + 1]);
                c = (char)u;
                s += c;
            }
            return s;
        }
        void GetEmail(NetworkStream stream, int n)
        {
            n *= 2;
            byte[] b = new byte[n];
            int read = stream.Read(b, 0, n);
            Debug.Log("length " + read + " / " + n);
            string s = ByteToString(b, n);
            Debug.Log("length: " + s.Length);
            Debug.Log("content: " + s);
            string[] ss = s.Split(":");
            if (ss.Length >= 2)
            {
                Debug.Log(ss[1]);
                core.SendProject(ss[0], "Tames project: " + ss[0], ss[1]);
            }
        }
        void GetRegister(NetworkStream stream, int n)
        {
            n *= 2;
            byte[] b = new byte[n];
            int read = stream.Read(b, 0, n);
            Debug.Log("length " + read + " / " + n);
            string s = ByteToString(b, n);
            int space = s.IndexOf(' ');
            if (space > 0)
            {
                string email = s.Substring(0, space);
                s = s.Substring(space + 1);
                space = s.IndexOf(' ');
                if (space > 0)
                {
                    string token = s.Substring(0, space);
                    string project = s.Substring(space + 1);
                    EmailData ed;
                    if ((ed = EmailData.Add(email, project, token, out bool result)) != null)
                    {
                        SendRegister(ed.owner.email, project, result);
                        Debug.Log("mail to " + ed.owner.email);
                    }
                }
            }
        }
        private static void SendRegister(string email, string project, bool result)
        {
            string body = result ? "Porject " + project + " was added successfully."
                : "Cannot add project " + project + ". The user has already reached its registry limit.";
            TCPServer.core.SendEmail(email, result ? "Tames: Project added" : "Tames: Error", body);

        }
    }
}
