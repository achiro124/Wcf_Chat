using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChatClient.ServiceChat;
using Wcf_Chat;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IServiceChatCallback
    {

        private Dictionary<TabItem, int> tabItems = new Dictionary<TabItem, int>();
        private Dictionary<ListBox, int> listBoxes = new Dictionary<ListBox, int>();
        private Dictionary<TextBox, int> textBoxes = new Dictionary<TextBox, int>();
        private Dictionary<int,string> listUsers = new Dictionary<int,string>();

        ServiceChatClient client;
        int ID;
        int toID;
        string name;
        public MainWindow(string name)
        {
            InitializeComponent();
            this.name = name;
            //this.ID = ID;
        }
        private void lbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in tabItems)
            {
                if (item.Key.Header.ToString() == lbUsers.Items[lbUsers.SelectedIndex].ToString())
                {
                    tcChat.SelectedIndex = tabItems.ToList().IndexOf(item) + 1;
                    lbUsers.SelectionChanged -= lbUsers_SelectionChanged;
                    lbUsers.SelectedIndex = -1;
                    lbUsers.SelectionChanged += lbUsers_SelectionChanged;
                    return;
                }

            }
            ListBox chatbox = new ListBox();
            TextBox textBox = new TextBox()
            {
                Name = "tb" + listUsers.FirstOrDefault(x => x.Value == lbUsers.Items[lbUsers.SelectedIndex].ToString()).Key.ToString()
            };
            textBox.KeyDown += tbMessage_KeyDown;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.3, GridUnitType.Star) });

            grid.Children.Add(chatbox);
            grid.Children.Add(textBox);
            Grid.SetRow(chatbox, 0);
            Grid.SetRow(textBox, 1);

            toID = listUsers.FirstOrDefault(x => x.Value == lbUsers.Items[lbUsers.SelectedIndex].ToString()).Key;


            listBoxes[chatbox] = toID;
            textBoxes[textBox] = toID;

            TabItem tabItem = new TabItem
            {
                Header = lbUsers.Items[lbUsers.SelectedIndex].ToString(),
                Height = 30,
                MinWidth = 80,
                Content = grid,
                Name = "tabItem" + listUsers.FirstOrDefault(x => x.Value == lbUsers.Items[lbUsers.SelectedIndex].ToString()).Key.ToString()
            };
            tabItems[tabItem] = toID;
            tcChat.Items.Add(tabItem);
            tcChat.SelectedIndex = tcChat.Items.Count - 1;


            
            lbUsers.SelectionChanged -= lbUsers_SelectionChanged;
            lbUsers.SelectedIndex = -1;
            lbUsers.SelectionChanged += lbUsers_SelectionChanged;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Disconnect(ID);
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                foreach (var item in textBoxes)
                {
                    if (textBox.Name == item.Key.Name)
                    {

                        if (client != null)
                        {
                            client.SendMsg(item.Key.Text, ID, toID);
                            item.Key.Text = string.Empty;
                        }

                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
            ID = client.Connect(name);
            client.GetUsers();
            tbName.Text = "Login: " + name;
        }

        public void MsgCallback(string msg, int fromId, int ToId)
        {
            foreach(var item in tabItems)
            {
                if(ToId == tabItems[item.Key] || fromId == tabItems[item.Key])
                {
                    if(fromId == ID)
                    {
                        var s = msg.Split(' ');
                        s[1] = " Вы: ";
                        msg = "";
                        foreach(var item1 in s)
                        {
                            msg += item1;
                        }
                    }
                    listBoxes.FirstOrDefault(x => x.Value == ToId || x.Value == fromId).Key.Items.Add(msg);
                    break;
                }
            }
        }

        public void UsersCallback(string[] names, int[] listId)
        {
            for(int i = 0; i < names.Length; i++)
            {
                if (!lbUsers.Items.Contains(names[i]) && this.name != names[i])
                {
                    lbUsers.Items.Add(names[i]);
                    listUsers.Add(listId[i], names[i]);
                }
            }
        }

        private void tcChat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as TabControl; 
            if (item != null && item.SelectedIndex != 0) 
            {
                toID =  tabItems.Values.ToList()[item.SelectedIndex - 1];
            }
             
        }

        private void bDisconnect_Click(object sender, RoutedEventArgs e)
        {
            client.Disconnect(ID);
        }

        public void DisconnectCallback(int id)
        {
            if(id == ID)
            {
                Chat chat = new Chat();
                chat.Show();
                this.Close();
            }
            else
            {

                if(listUsers.FirstOrDefault(x => x.Key == id).Value != null)
                    lbUsers.Items.Remove(listUsers.FirstOrDefault(x => x.Key == id).Value);
                if (tabItems.FirstOrDefault(x => x.Value == id).Key != null)
                {
                    tcChat.Items.Remove(tabItems.FirstOrDefault(x => x.Value == id).Key);
                    tcChat.SelectedIndex = 0;
                }
                if (tabItems.FirstOrDefault(x => x.Value == id).Key != null)
                    tabItems.Remove(tabItems.FirstOrDefault(x => x.Value == id).Key);
                if(listBoxes.FirstOrDefault(x => x.Value == id).Key != null)
                    listBoxes.Remove(listBoxes.FirstOrDefault(x => x.Value == id).Key);
                if(textBoxes.FirstOrDefault(x => x.Value == id).Key != null)
                    textBoxes.Remove(textBoxes.FirstOrDefault(x=>x.Value == id).Key);
                listUsers.Remove(id);
                
            }
        }

        //  public void AllMsgsCallback(PrivateMessage[] allMsgs)
        //  {
        //      for (int i = 0; i < tabItems.Count; i++)
        //      {
        //          if (toID.ToString() == tabItems[i].Name.Remove(0, 7) || ID.ToString() == tabItems[i].Name.Remove(0, 7))
        //          {
        //              foreach(var item in allMsgs)
        //              {
        //                  listBoxes[i].Items.Add(item.Text);
        //              }
        //              break;
        //          }
        //      }
        //  }
    }
}
