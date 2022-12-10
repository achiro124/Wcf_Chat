using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Wcf_Chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChat : IServiceChat
    {
        ApplicationContext db = new ApplicationContext();
        List<ServerUser> users = new List<ServerUser>();
        int nextId = 1;
        public int Connect(string name)
        {
            foreach(var user1 in users)
            {
                if (user1.Name == name)
                {
                    return -1;
                }
            }
            ServerUser user = new ServerUser
            {
                ID = nextId,
                Name = name,
                operationContext = OperationContext.Current
            };
            nextId++;
            
            users.Add(user);
            return user.ID;
        }

        public void Disconnect(int id)
        {
            var user = users.FirstOrDefault(x => x.ID == id);
            if (user != null)
            {
                foreach (var item in users)
                {
                    item.operationContext.GetCallbackChannel<IServerChatCallback>().DisconnectCallback(id);
                }
                users.Remove(user);
            }

        }

     //   public void GetAllMsgs(int ToId, int fromId)
     //   {
     //       db.Database.EnsureCreated();
     //       db.Msgs.Load();
     //       var messages = db.Msgs;
     //       users.FirstOrDefault(x => x.ID == fromId).operationContext.GetCallbackChannel<IServerChatCallback>().AllMsgsCallback(messages.Where(x => x.From == fromId && x.To == ToId).ToList());
     //   }

        public void GetUsers()
        {
            List<string> names = new List<string>();
            List<int> listId = new List<int>();
            foreach (var item in users)
            {
                names.Add(item.Name);
                listId.Add(item.ID);
            }
            foreach (var item in users)
            {
                item.operationContext.GetCallbackChannel<IServerChatCallback>().UsersCallback(names, listId);
            }
        }

        public void SendMsg(string msg, int fromId, int ToId)
        {
            //SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
          //  db.Database.EnsureCreated();
          //  db.Msgs.Add(new PrivateMessage
          //  {
          //      dateTime = DateTime.Now,
          //      From = fromId,
          //      To = ToId,
          //      Text= msg
          //  });
          //  db.SaveChanges();

            string answer = DateTime.Now.ToShortTimeString() + " ";
            answer += users.FirstOrDefault(x => x.ID == fromId).Name + ": ";
            answer += msg;
            foreach (var item in users.Where(x => x.ID == fromId || x.ID == ToId))
            {
                item.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(answer, fromId, ToId);
            }
        }
    }
}
