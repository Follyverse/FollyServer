using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
namespace Multi
{
    public class RegisterData
    {
        public static List<RegisterData> Owners = new List<RegisterData>();
        public string token;
        public int count;
        public int current = 0;
        public string email;
        public static void ReadAll()
        {
            if (File.Exists(CoreTame.applicationPath + "owners.txt"))
            {
                string[] lines = File.ReadAllLines(CoreTame.applicationPath + "owners.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] s = lines[i].Split(' ');
                    if (s.Length > 1)
                    {
                        try
                        {
                            int count = int.Parse(s[1]);
                            Owners.Add(new RegisterData() { token = s[0], count = count, email = s[2] });
                        }
                        catch (Exception e) { }
                    }
                }
                Debug.Log("owners " + Owners.Count);
            }
        }
        public static void Add(string token, int count, string email)
        {
            RegisterData rd = Owners.Find(x => x.token == token);
            if (rd != null) rd.count = count;
            else
                Owners.Add(new RegisterData() { token = token, count = count, email = email });

        }
        public static void WriteAll()
        {
            string[] lines = new string[Owners.Count];
            for (int i = 0; i < lines.Length; i++)
                lines[i] = Owners[i].token + " " + Owners[i].count + " " + Owners[i].email;
            File.WriteAllLines(CoreTame.applicationPath + "owners.txt", lines);
        }
    }
    public class EmailData
    {
        public static List<EmailData> Emails = new List<EmailData>();
        public string email;
        public string id;
        public string token;
        public RegisterData owner;
        public static void ReadAll()
        {
            if (File.Exists(CoreTame.applicationPath + "emails.txt"))
            {
                string[] lines = File.ReadAllLines(CoreTame.applicationPath + "emails.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] s = lines[i].Split('|');
                    EmailData ed = new EmailData() { email = s[0], id = s[1], token = s[2] };
                    RegisterData rd = RegisterData.Owners.Find(x => x.token == s[2]);
                    if (rd != null)
                    {
                        Emails.Add(ed);
                        ed.owner = rd;
                        rd.current++;
                    }
                }
                Debug.Log("Emails "+Emails.Count);
            }
        }
        public static EmailData Add(string email, string id, string token, out bool success)
        {
            EmailData ed = null;
            RegisterData rd = RegisterData.Owners.Find(x => x.token == token);
            if (rd == null) success = false;
            else if (rd.current == rd.count - 1) success = false;
            else
            {
                rd.current++;
                ed = new EmailData() { email = email, id = id, token = token, owner = rd };
                Emails.Add(ed);
                success = true;
            }
            return ed;
        }
        public static void WriteAll()
        {
            string[] lines = new string[Emails.Count];
            for (int i = 0; i < lines.Length; i++)
                lines[i] = Emails[i].email + "|" + Emails[i].id + "|" + Emails[i].token;
            File.WriteAllLines(CoreTame.applicationPath + "emails.txt", lines);
        }
    }
}

