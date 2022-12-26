using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;

namespace Wcf_Chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChat : IServiceChat
    {

        SqlConnection connection;
        SqlCommand sqlCommand;
        SqlConnectionStringBuilder connectionStringBuilder;
        List<ServerUser> users = new List<ServerUser>();
        int nextId = 1;
        public ServiceChat() 
        {
            ConnectToDb();
            ConnectToUserBd();
        }

        private void ConnectToDb()
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

        private void ConnectToUserBd()
        {
            try
            {
                sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Users";
                sqlCommand.CommandType = System.Data.CommandType.Text;

                connection.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new ServerUser
                    {
                        ID = Convert.ToInt32(reader[0]),
                        Login = reader[1].ToString(),
                    });
                    nextId = Convert.ToInt32(reader[0]) + 1;
                }
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


        public int Connect(string name)
        {
            int id = -1;
            ServerUser serverUser = users.FirstOrDefault(x => x.Login == name);
            if(serverUser != null && serverUser.Active)
            {
                return id;
            }
            else
            {
                if(serverUser != null)
                {
                    id = serverUser.ID;
                    serverUser.Active = true;
                    serverUser.operationContext = OperationContext.Current;
                }
                else
                {
                    try
                    {
                        serverUser = new ServerUser
                        {
                            ID = nextId,
                            Login = name,
                            Active = true,
                            operationContext = OperationContext.Current
                        };
                        id = serverUser.ID;
                        users.Add(serverUser);
                        nextId++;

                        sqlCommand = connection.CreateCommand();
                        sqlCommand.CommandText = "INSERT INTO Users VALUES(@Id, @Login)";
                        sqlCommand.Parameters.AddWithValue("Id", serverUser.ID);
                        sqlCommand.Parameters.AddWithValue("Login", serverUser.Login);
                        sqlCommand.CommandType = System.Data.CommandType.Text;
                        connection.Open();
                        sqlCommand.ExecuteNonQuery();
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

            }

            return id;
        }

        public void Disconnect(int id)
        {
            var user = users.FirstOrDefault(x => x.ID == id);
            user.Active = false;
           // if (user != null)
           // {
           //     foreach (var item in users)
           //     {
           //         //item.operationContext = OperationContext.Current;
           //         //item.operationContext.GetCallbackChannel<IServerChatCallback>().DisconnectCallback(id);
           //
           //     }
           //     users.Remove(user);
           // }

        }

        public void GetAllMsgs(int ToId, int fromId)
        {
            List<string> listMessage = new List<string>();
            try
            {
                sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "SELECT Msg FROM PrivateMessage1 WHERE ToId = @ToId AND FromId = @FromId OR ToId = @FromId AND FromId = @ToId ORDER BY DateTime ASC";
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
                names.Add(item.Login);
                listId.Add(item.ID);
            }
            foreach (var item in users.Where(x => x.Active))
            {
                //item.operationContext = OperationContext.Current;
                item.operationContext.GetCallbackChannel<IServerChatCallback>().UsersCallback(names, listId);
            }

        }

        public void SendMsg(PrivateMessage privateMessage)
        {

            try
            {
                sqlCommand = connection.CreateCommand();
                string answer = DateTime.Now.ToShortTimeString() + " ";
                answer += users.FirstOrDefault(x => x.ID == privateMessage.FromId).Login + ": ";
                answer += privateMessage.Msg;
                privateMessage.Msg = answer;

                sqlCommand.CommandText = "INSERT INTO PrivateMessage1 VALUES(@Id, @FromId, @ToId, @Msg, @DateTime)";
                sqlCommand.Parameters.AddWithValue("Id", privateMessage.ID);
                sqlCommand.Parameters.AddWithValue("FromId", privateMessage.FromId);
                sqlCommand.Parameters.AddWithValue("ToId", privateMessage.ToId);
                sqlCommand.Parameters.AddWithValue("Msg", privateMessage.Msg);
                sqlCommand.Parameters.AddWithValue("DateTime", privateMessage.dateTime);

                sqlCommand.CommandType = System.Data.CommandType.Text;
                connection.Open();


                foreach (var item in users.Where(x => x.ID == privateMessage.FromId && x.Active || x.ID == privateMessage.ToId && x.Active))
                {
                    //item.operationContext = OperationContext.Current;
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
