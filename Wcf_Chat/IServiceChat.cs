using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Wcf_Chat
{
    [ServiceContract(CallbackContract = typeof(IServerChatCallback))]
    public interface IServiceChat
    {
        [OperationContract]
        int Connect(string name);

        [OperationContract(IsOneWay = true)]
        void Disconnect(int id);

        [OperationContract(IsOneWay = true)]
        void SendMsg(string msg, int fromId,int ToId);

        [OperationContract(IsOneWay = true)]
        void GetUsers();

     //   [OperationContract(IsOneWay = true)]
     //   void GetAllMsgs(int ToId, int fromId);
    }

    public interface IServerChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void MsgCallback(string msg,int fromId, int ToId);

        [OperationContract(IsOneWay = true)]
        void UsersCallback(List<string> names, List<int> listId);
        [OperationContract(IsOneWay = true)]
        void DisconnectCallback(int id);

     //   [OperationContract(IsOneWay = true)]
     //   void AllMsgsCallback(List<PrivateMessage> allMsgs);

    }
}
