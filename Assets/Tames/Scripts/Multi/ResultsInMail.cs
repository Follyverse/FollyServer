using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiptideNetworking;
using System.Net;
using System.Net.Mail;

namespace Multi
{
    public enum ResultType { Choice, Progress, Alter }
    public class ResultsInMail
    {
        public RemoteProject project;
        public PersonClient person;
        public string uniqueID;
        public List<SendingItem> items = new List<SendingItem>();
        public ResultsInMail(RemoteProject proj, PersonClient pers)
        {
            project = proj;
            person = pers;
            uniqueID = person.id + "." + person.ConnectionTime;
        }
        public void FromMessage(Message m)
        {
            int cc = m.GetInt();
            int pc = m.GetInt();
            int ac = m.GetInt();
            for (int i = 0; i < cc; i++)
                items.Add(new SendingItem(m.GetString(), m.GetInt(), ResultType.Choice));
            for (int i = 0; i < pc; i++)
                items.Add(new SendingItem(m.GetString(), m.GetFloat()));
            for (int i = 0; i < ac; i++)
                items.Add(new SendingItem(m.GetString(), m.GetInt(), ResultType.Alter));
        }
        public List<string> ToLine()
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < items.Count; i++)
                lines.Add(i + "\t" + items[i].type + '\t' + items[i].Value + '\t' + items[i].name);
            return lines;
        }
        public void Send()
        {
            List<string> lines = new List<string>();
            lines.Add(project.name);
            lines.Add(project.id);
            lines.Add(uniqueID);
            lines.AddRange(ToLine());


            string username = "molabfacvut@gmail.com";
            string password = "irxddjjxkhlkkkkg";
            string subject = "Survey results: " + project.name;
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
            };
            string s = "";
            for (int i = 0; i < lines.Count; i++)
                s += lines[i] + "\r\n";
            smtpClient.Send(username, project.recipient, subject, s);
        }
    }


    public class SendingItem
    {
        public ResultType type;
        public string name;
        public string Value { get { return "" + (type == ResultType.Progress ? floatValue : intValue); } }
        private float floatValue;
        private int intValue;
        public SendingItem(string name, float value)
        {
            this.name = name;
            type = ResultType.Progress;
            floatValue = value;
        }
        public SendingItem(string name, int value, ResultType type)
        {
            this.name = name;
            this.type = type;
            intValue = value;
        }
    }
}
