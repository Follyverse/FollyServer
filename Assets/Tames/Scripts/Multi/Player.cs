
using Multi;
using RiptideNetworking;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //    public static ushort LocalID = ushort.MaxValue - 1;
    //    public static ushort ServerID = ushort.MaxValue - 2;
    public static ushort NoID = ushort.MaxValue - 3;
    //  public static RemoteProject project;
    public static PersonClient[] users = new PersonClient[ushort.MaxValue - 10];

    public const ushort Name = 255;

    public const ushort S_FrameData = 1;
    public const ushort C_FrameData = 101;
    public const ushort S_ID = 2;
    public const ushort C_ID = 102;
    public const ushort S_Disconnect = 3;
    public const ushort C_Disconnect = 103;
    public const ushort S_Capacity = 4;
    public const ushort S_ReadyToGo = 5;
    public const ushort C_ReadyToGo = 105;
    public const ushort C_EmailToOwner = 6;
    public const ushort C_Properties = 107;
    public const ushort S_Properties = 7;
    public const ushort C_Nickname = 108;
    public const ushort S_Nickname = 8;
    public const ushort C_RequestStatus = 109;
    public const ushort C_SendStatus = 110;
    public const ushort S_RequestStatus = 9;
    public const ushort S_SendStatus = 10;
    public const ushort C_Register = 120;
    public const ushort C_Token = 200;
    //  public static PersonClient[] users = new PersonClient[1 << 15];
    public static int connected = 0;
    static TimeSpan MaxInactive = TimeSpan.FromMinutes(5);
    //    public static List<Person> users = new List<Person>();

    [MessageHandler(Name)]
    private static void PersonConnected(ushort id, Message m)
    {
        Debug.Log("Name " + id);
        //ushort id = m.GetUShort();
        SendID(id);
    }
    public static void ReachedCapacity(ushort id, string s)
    {
        Message m = Message.Create(MessageSendMode.reliable, S_Capacity);
        m.AddString(s);
        NetworkManager.Singleton.Server.Send(m, id);
    }
    private static void SendID(ushort id)
    {
        Message m = Message.Create(MessageSendMode.reliable, S_ID);
        m.AddUShort((ushort)id);
        Debug.Log("id sent " + id);
        NetworkManager.Singleton.Server.Send(m, id, false);
    }
    [MessageHandler(C_Properties)]
    private static void GetProperties(ushort id, Message m)
    {
        string projectID = m.GetString();
        string name = m.GetString();
        string nickname = m.GetString();
        string mail = m.GetString();
        RemoteProject project = CoreTame.FindByIndex(projectID, name, out bool created);
        Debug.Log(projectID + " " + project.index + " " + project.users.Count);
        if (project.recipient == "") project.recipient = mail;
        PersonClient person = new PersonClient(id) { project = project, connection = DateTime.Now, nickname = nickname };
        users[id] = person;
        // project.users.Add(person);
        if (!created || project.users.Count > 1)
            RequestStatus(project, id);
        else
        {
            project.users.Add(person);
            SendReady(id);
        }
    }
    public static void SendReady(ushort id)
    {
        Debug.Log("Ready ");
        Message m = Message.Create(MessageSendMode.reliable, S_ReadyToGo);
        NetworkManager.Singleton.Server.Send(m, id);
    }
    public static void RequestStatus(RemoteProject project, ushort id)
    {
        DateTime now = DateTime.Now;
        PersonClient p = null;
        //    Message m;
        users[id].awaitingRequest = true;
        users[id].requestTime = DateTime.Now;
        foreach (PersonClient person in project.users)
            if (person.id != id)
            {
                Message m = Message.Create(MessageSendMode.reliable, S_RequestStatus);
                m.AddUShort(id);
                NetworkManager.Singleton.Server.Send(m, person.id);

            }
        //   RequestProgress(id);
    }
    [MessageHandler(C_SendStatus)]
    private static void ReceiveStatus(ushort sender, Message m)
    {
        int cf, ci;
        ushort id = m.GetUShort();
        if (users[id] != null)
            if (users[id].awaitingRequest)
            {
                users[id].awaitingRequest = false;
                if (DateTime.Now - users[id].requestTime < TimeSpan.FromSeconds(30))
                {
                    Message m2 = Message.Create(MessageSendMode.reliable, S_SendStatus);
                    m2.AddInt(cf = m.GetInt());
                    m2.AddInt(ci = m.GetInt());
                    for (int i = 0; i < cf; i++)
                    {
                        byte b = m.GetByte();
                        m2.AddByte(b);
                        switch (b)
                        {
                            case 0: break;
                            case 1:
                                m2.AddFloat(m.GetFloat());
                                m2.AddFloat(m.GetFloat());
                                m2.AddFloat(m.GetFloat());
                                m2.AddInt(m.GetInt());
                                m2.AddInt(m.GetInt());
                                m2.AddInt(m.GetInt());
                                break;
                            case 2:
                                m2.AddFloat(m.GetFloat());
                                m2.AddFloat(m.GetFloat());
                                break;
                        }
                    }
                    for (int i = 0; i < ci; i++)
                    {
                        m2.AddByte(m.GetByte());
                        m2.AddInt(m.GetInt());
                    }
                    NetworkManager.Singleton.Server.Send(m2, id);
                }
            }
    }
    [MessageHandler(C_ReadyToGo)]
    private static void ReceiveReady(ushort id, Message m)
    {
        if (users[id] != null)
            users[id].project.users.Add(users[id]);
        SendReady(id);
    }

    public static void SendDisconnect(RemoteProject project, ushort id)
    {
        Message m;
        foreach (PersonClient person in project.users)
        {
            m = Message.Create(MessageSendMode.reliable, S_Disconnect);
            m.AddUShort(id);
            NetworkManager.Singleton.Server.Send(m, person.id);
        }
    }
    [MessageHandler(C_FrameData)]
    private static void ReceiveFrame(ushort id, Message m)
    {
        PersonClient person = users[id];
        if (person != null)
            person.RecevieFrameAsServer(m);
    }

    [MessageHandler(C_Nickname)]
    private static void ReceiveNicknameRequest(ushort sender, Message m)
    {
        ushort reqID = m.GetUShort();
        Debug.Log("Mickname " + sender + " " + reqID);
        PersonClient person = users[reqID];
        if (person != null)
        {
            Message reply = Message.Create(MessageSendMode.reliable, S_Nickname);
            reply.Add(reqID);
            reply.Add(person.nickname);
            NetworkManager.Singleton.Server.Send(reply, sender);
        }
    }
    public static void Disconnect(ushort id)
    {
        if (id < users.Length && id >= 0)
        {
            PersonClient person = users[id];
            if (person != null)
            {
                if (person.toEmail != null)
                    person.toEmail.Send();
                if (person.project != null) person.project.Disconnect(person);
                users[id] = null;
            }
        }
    }


    [MessageHandler(C_Register)]
    private static void RegisterMail(ushort id, Message m)
    {

        string project = m.GetString();
        string email = m.GetString();
        string token = m.GetString();
        Debug.Log("mail register " + project + " : " + email);
        EmailData ed;
        if ((ed = EmailData.Add(email, project, token, out bool result)) != null)
        {
         //   SendRegister(ed.owner.email, project, result);
            Debug.Log("mail to " + ed.owner.email);
        }
        Debug.Log(ed == null ? "failure" : "success");
        
    }
    [MessageHandler(C_Token)]
    private static void RegisterToken(ushort id, Message m)
    {

        string password = m.GetString();
        string token = m.GetString();
        string email = m.GetString();
        int count = m.GetInt();
        if (password == TCPServer.Authenticator)
        {
            RegisterData.Add(token, count, email);
            TCPServer.core.SendEmail(email, "Tames: Token added", "token " + token + " was registered with " + email);
        }
        else
            TCPServer.core.SendEmail(TCPServer.Email, "Tames: Wrong password", "token " + token + " could not be registered due to the wrong password.");
    }

    


}
