using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Wcf_Chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChat : IServiceChat
    {

        public ServiceChat() 
        {
            ConnectToDb();
        }

        SqlConnection connection;
        SqlCommand sqlCommand;

        SqlConnectionStringBuilder connectionStringBuilder;

        void ConnectToDb()
        {
            connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = "DESKTOP-FGS36Q5";
            connectionStringBuilder.InitialCatalog = "DataBase";
            connectionStringBuilder.Encrypt = true;
            connectionStringBuilder.TrustServerCertificate = true;
            connectionStringBuilder.ConnectTimeout = 30;
            connectionStringBuilder.AsynchronousProcessing = true;
            connectionStringBuilder.MultipleActiveResultSets= true;
            connectionStringBuilder.IntegratedSecurity = true;


            connection = new SqlConnection(connectionStringBuilder.ToString());
        }

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

        public void GetAllMsgs(int ToId, int fromId)
        {
            List<string> listMessage = new List<string>();
            try
            {
                sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "SELECT Msg FROM PrivateMessage WHERE ToId = @ToId AND FromId = @FromId OR ToId = @FromId AND FromId = @ToId";
                sqlCommand.Parameters.AddWithValue("ToId", ToId);
                sqlCommand.Parameters.AddWithValue("FromId", fromId);
                sqlCommand.CommandType = System.Data.CommandType.Text;

                connection.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read()) 
                {
                    listMessage.Add(reader[0].ToString());
                }

                users.FirstOrDefault(x => x.ID == fromId).operationContext.GetCallbackChannel<IServerChatCallback>().AllMsgsCallback(listMessage);

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
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

        public void SendMsg(PrivateMessage privateMessage)
        {

            try
            {
                sqlCommand = connection.CreateCommand();
                string answer = DateTime.Now.ToShortTimeString() + " ";
                answer += users.FirstOrDefault(x => x.ID == privateMessage.FromId).Name + ": ";
                answer += privateMessage.Msg;
                privateMessage.Msg = answer;

                sqlCommand.CommandText = "INSERT INTO PrivateMessage VALUES(@Id, @FromId, @ToId, @Msg, @DateTime)";
                sqlCommand.Parameters.AddWithValue("Id", privateMessage.ID);
                sqlCommand.Parameters.AddWithValue("FromId", privateMessage.FromId);
                sqlCommand.Parameters.AddWithValue("ToId", privateMessage.ToId);
                sqlCommand.Parameters.AddWithValue("Msg", privateMessage.Msg);
                sqlCommand.Parameters.AddWithValue("DateTime", privateMessage.dateTime);

                sqlCommand.CommandType = System.Data.CommandType.Text;
                connection.Open();


                foreach (var item in users.Where(x => x.ID == privateMessage.FromId || x.ID == privateMessage.ToId))
                {
                    item.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(privateMessage);
                }

                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if(connection != null)
                {
                    connection.Close();
                }
            }


        }
    }
}
