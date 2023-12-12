using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RiptideNetworking;
namespace Multi
{
    public class InputRecord
    {
        public const byte Head = 0;
        public const byte Hand = 1;
        public const byte Grip = 3;
        public const byte Trigger = 4;
        public const byte VR = 8;
        public const byte GP = 7;
        public const byte KB = 5;
        public const byte Mouse = 6;

        public const byte Position = 1;
        public const byte Rotation = 2;
        public const bool Hold = true;
        public const byte Shift = 1;
        public const byte Ctrl = 2;
        public const byte Alt = 4;
        public const byte LS = 1;
        public const byte RS = 2;

        public byte type;
        public byte subtype;
        public byte aux;
        public byte index;
        public bool hold;
        public float[] value;
        public InputRecord() { }
        public InputRecord(Vector3 p, byte t = 0)
        {
            if (t == 0)
                type = Head;
            else
            { type = Hand; index = (byte)(t - 1); }
            subtype = Position;
            value = new float[] { p.x, p.y, p.z };
        }
        public InputRecord(Quaternion q, byte t = 0)
        {
            if (t == 0)
                type = Head;
            else
            { type = Hand; index = (byte)(t - 1); }
            subtype = Rotation;
            value = new float[] { q.x, q.y, q.z, q.w };
        }
        public void FromVector(Vector3 v)
        {
            value[0] = v.x;
            value[1] = v.y;
            value[2] = v.z;
        }
        public void FromQuaternion(Quaternion q)
        {
            value[0] = q.x;
            value[1] = q.y;
            value[2] = q.z;
            value[3] = q.w;
        }
        public bool AddToMessage(Message m)
        {
            bool r = false;
            switch (type)
            {
                case Head:
                    m.AddByte(Head);
                    if (subtype == Position)
                        m.AddVector3(new Vector3(value[0], value[1], value[2]));
                    else
                        m.AddQuaternion(new Quaternion(value[0], value[1], value[2], value[3]));
                    break;
                case Hand:
                    m.AddByte((byte)(index + 1));
                    switch (subtype)
                    {
                        case Position: m.AddVector3(new Vector3(value[0], value[1], value[2])); break;
                        case Rotation: m.AddQuaternion(new Quaternion(value[0], value[1], value[2], value[3])); break;
                        case Grip: m.AddFloat(value[0]); break;
                        case Trigger: m.AddFloat(value[0]); break;
                    }
                    break;
                case KB:
                case GP:
                case VR:
                    m.AddByte(type);
                    m.AddByte(index);
                    m.AddByte(aux);
                    m.AddBool(hold);
                    break;
            }
            return true;
        }
        public static void AddAll(Message m, ushort id, List<InputRecord> rec)
        {
            m.Add(id);
            for (int i = 0; i < rec.Count; i++)
                rec[i].AddToMessage(m);
            m.AddByte(255);
        }
    }
}
