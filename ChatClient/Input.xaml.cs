using ChatClient.ServiceChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatClient
{
    public partial class Chat : Window
    {
        //ServiceChatClient client;
        public Chat()
        {
            InitializeComponent();
        }

    //    public void MsgCallback(string msg, int fromId, int ToId)
    //    {
    //        throw new NotImplementedException();
    //    }
    //
    //    public void UsersCallback(string[] names, int[] listId)
    //    {
    //        throw new NotImplementedException();
    //    }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // int ID = client.Connect(tbUserName.Text);
           // if(ID == -1)
           // {
           //     msgErr.Content = "Пользователь с таким логином уже существует";
           // }
          //  else
          //  {
                MainWindow chat = new MainWindow(tbUserName.Text);
                chat.Show();
                this.Close();
           // }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
        }
    }
}
