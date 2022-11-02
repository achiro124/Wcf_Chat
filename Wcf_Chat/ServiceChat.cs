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
        List<ServerUser> users = new List<ServerUser>();
        int nextId = 1;
        public int Connect(string name)
        {
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
                users.Remove(user);
            }
        }

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
            foreach (var item in users.Where(x => x.ID == fromId || x.ID == ToId))
            {
                string answer = DateTime.Now.ToShortTimeString();
                answer += ": " + users.FirstOrDefault(x => x.ID == fromId).Name + " ";
                answer += msg;
                item.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(answer, fromId, ToId);
            }
        }
    }
}
