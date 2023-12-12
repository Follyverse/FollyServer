using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RiptideNetworking;
using UnityEngine;
namespace Multi
{
    public enum RemoteType
    {
        Student, Research, Construction
    }

    public class RemoteProject
    {
        public string name;
        public string id;
        public int index;
        public long created;
        public long lastModified;
        public string description;
        public string recipient;
        public List<PersonClient> users = new List<PersonClient>();
        public int maxUsers = 255;
        public DateTime lastChecked;
       
        public PersonClient FindByID(ushort id, out int index)
        {
            index = -1;
            for (int i = 0; i < users.Count; i++)
                if (id == users[i].id)
                {
                    index = i;
                    return users[i];
                }
            return null;
        }
        public void Disconnect(PersonClient p)
        {
            FindByID(p.id, out int index);
            if (index >= 0 && index < users.Count)
            {
                users.RemoveAt(index);
                Player.SendDisconnect(p.project, p.id);
                if(users.Count == 0)
                    CoreTame.projects.Remove(this);
            }
        }
        public bool CheckConnection()
        {
            if (DateTime.Now - lastChecked > TimeSpan.FromMinutes(1))
            {
                lastChecked = DateTime.Now;
                for (int i = users.Count - 1; i >= 1; i--)
                    if (DateTime.Now - users[i].lastSignal > TimeSpan.FromSeconds(300))
                        Disconnect(users[i]);
            }
            return users.Count > 0;

        }

        public bool SendFrameAsServer()
        {
            //    Flush();
            bool r = CheckConnection(); //      Player.project.CheckConnection();
      //      Debug.Log("users: "+users.Count);
            bool reliable = false;
            for (int i = 0; i < users.Count; i++)
                if (users[i].reliable) { reliable = true; break; }
            Message m = Message.Create(reliable ? MessageSendMode.reliable : MessageSendMode.unreliable, Player.S_FrameData);
            m.AddByte((byte)users.Count);
            byte playerAdded = 0;
            bool pp=false;
            //     bool[] common, input;
       //     Debug.Log("checking");
            for (int i = 0; i < users.Count; i++)
            {
                PersonClient pc = users[i];
                pc.ClearRecord();
                byte sendable = pc.sendableCount;
                if (sendable == 0)
                    m.AddUShort(Player.NoID);
                else
                {
                    m.AddUShort(pc.id);
               //     Debug.Log("id = "+pc.id);
                    playerAdded++;
                    m.AddByte(sendable);
                    for (int j = 0; j < 6; j++) if (pc.sendableCommon[j]) pc.AddCommon(m, j);
                    for (int j = 0; j < 3; j++) if (pc.sendableInput[j])
                        {
                            pp = true;
                            pc.AddInput(m, j, false);
                 //           Debug.Log(pc.id + " sending pressed");
                        }
                    for (int j = 0; j < 3; j++) if (pc.sendableInput[j + 3]) pc.AddInput(m, j, true);
                    if (pc.sendableInput[6]) pc.AddAux(m);
                    if (pc.sendableInput[7]) pc.AddGrip(m);
                }
                //         if (i != 0) pc.ClearRecord();
            }
            if (playerAdded != 0)
            {
        //        if (pp) Debug.Log("added" + playerAdded);
                for (int i = 0; i < users.Count; i++)
                    NetworkManager.Singleton.Server.Send(m, users[i].id);
            } return r;
        }
    }
}
