using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Multi;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
public class CoreTame : MonoBehaviour
{
    public bool VRMode;
    public static bool multiPlayer = false;
    public static List<RemoteProject> projects = new List<RemoteProject>();
    public static GameObject HeadObject;
    float timer = 0;
    public static string applicationPath;
    void Start()
    {
        TCPServer.core = this;
        smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(TCPServer.Email, TCPServer.Password),
            EnableSsl = true,
        };
         RegisterData.ReadAll();
        EmailData.ReadAll();
        for (int i = 0; i < Player.users.Length; i++)
            Player.users[i] = null;
    }
    void FixedUpdate()
    {
        if (Time.time - timer > 60)
        {
            timer = Time.time;
            SaveEmails();
        }
        for (int i = projects.Count - 1; i >= 0; i--)
            if (projects[i].users.Count > 0)
                if (!projects[i].SendFrameAsServer())
                    projects.RemoveAt(i);
    }

    public static RemoteProject FindByIndex(string id, string name, out bool created)
    {
        created = false;
        foreach (RemoteProject project in projects)
            if (project.id == id)
                return project;
        RemoteProject p = new RemoteProject() { id = id, name = name, index = projects.Count };
        projects.Add(p);
        created = true;
        return p;
    }
    static SmtpClient smtpClient;
    async public void SendProject(string project, string subject, string body)
    {
        EmailData ed = EmailData.Emails.Find(x => x.id == project);
        if (ed != null)
        {
            await Task.Run(() =>
            {
                try
                {
                    smtpClient.Send(TCPServer.Email, ed.email, subject, body);
                }
                catch { }
            });

        }
    }
    async public void SendEmail(string email, string subject, string body)
    {
        // EmailData ed = EmailData.Emails.Find(x => x.id == project);

        await Task.Run(() =>
        {
            try
            {
                smtpClient.Send(TCPServer.Email, email, subject, body);
            }
            catch { }
        });
    }
    void SaveEmails()
    {
        Debug.Log("saving emails " + EmailData.Emails.Count);
        StartCoroutine(WriteEmails());
    }
    IEnumerator WriteEmails()
    {
        EmailData.WriteAll();
        RegisterData.WriteAll();
        yield return null;
    }
}
