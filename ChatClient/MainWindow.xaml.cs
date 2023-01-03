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
        private Dictionary<Button, int> listButtons = new Dictionary<Button, int>();

        ServiceChatClient client;
        int ID;
        int toID;
        string name;
        public MainWindow(string name)
        {
            InitializeComponent();
            this.name = name;
        }
        private void lbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in tabItems)
            {
               // if (item.Key.Header.ToString() == lbUsers.Items[lbUsers.SelectedIndex].ToString())
               // {
               //     tcChat.SelectedIndex = tabItems.ToList().IndexOf(item) + 1;

               //     return;
               // }

                if(listUsers.FirstOrDefault(x => x.Value == lbUsers.Items[lbUsers.SelectedIndex].ToString()).Key == item.Value)
                {
                    tcChat.SelectedIndex = tabItems.ToList().IndexOf(item) + 1;
                    ListBox msgBox = listBoxes.FirstOrDefault(x => x.Value == toID).Key;
                    //msgBox.ScrollIntoView(msgBox.Items[msgBox.Items.Count - 1]);
                    return;
                }

            }


            toID = listUsers.FirstOrDefault(x => x.Value == lbUsers.Items[lbUsers.SelectedIndex].ToString()).Key;

            ListBox chatbox = new ListBox();
            TextBox textBox = new TextBox()
            {
                Name = "tb" + toID.ToString()
            };
            textBox.KeyDown += tbMessage_KeyDown;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.3, GridUnitType.Star) });
            grid.Children.Add(chatbox);
            grid.Children.Add(textBox);
            Grid.SetRow(chatbox, 0);
            Grid.SetRow(textBox, 1);

            


            listBoxes[chatbox] = toID;
            textBoxes[textBox] = toID;


            TextBlock textBlock = new TextBlock();
            textBlock.Text = lbUsers.Items[lbUsers.SelectedIndex].ToString();
            Button btclose = new Button();
            btclose.Content = "X";
            btclose.Margin = new Thickness(30,0,0,0);
            btclose.Click += Click_Butt_CloseTabItem;
            listButtons[btclose] = toID;

            Grid grid2 = new Grid();
            grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.3, GridUnitType.Star) });

            grid2.Children.Add(textBlock);
            grid2.Children.Add(btclose);
            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(btclose, 1);

            TabItem tabItem = new TabItem
            {
                Header = grid2,
                Height = 30,
                MinWidth = 80,
                Content = grid,
                Name = "tabItem" + toID.ToString()
            };
            tabItems[tabItem] = toID;
            tcChat.Items.Add(tabItem);

           // if(tcChat.SelectedIndex == 0)
           //     tcChat.SelectedIndex = tcChat.Items.Count - 1;


            
            client.GetAllMsgs(toID, ID);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Disconnect(ID);
        }

        private void Click_Butt_CloseTabItem(object sender, RoutedEventArgs e)
        {
            Button button= sender as Button;
            if(button != null)
            {
                int id = listButtons[button];
                if(tcChat.SelectedIndex != 0 && id == toID)
                {
                    tcChat.SelectedIndex = 0;
                }
                //tcChat.SelectedIndex = 0;
                tcChat.Items.Remove(tabItems.FirstOrDefault(x => x.Value == id).Key);

                tabItems.Remove(tabItems.FirstOrDefault(x => x.Value == id).Key);
                listBoxes.Remove(listBoxes.FirstOrDefault(x => x.Value == id).Key);
                textBoxes.Remove(textBoxes.FirstOrDefault(x => x.Value == id).Key);
                listButtons.Remove(listButtons.FirstOrDefault(x => x.Value == id).Key);
            }
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
                            PrivateMessage privateMessage = new PrivateMessage();
                            privateMessage.FromId = ID;
                            privateMessage.ToId = toID;
                            privateMessage.dateTime = DateTime.Now;
                            privateMessage.Msg = item.Key.Text;
                            client.SendMsg(privateMessage);
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
            if(ID == -1)
            {
                Chat input = new Chat();
                input.Show();
                this.Close();
            }
            client.GetUsers();
            tbName.Text = "Login: " + name;
        }

        public void MsgCallback(PrivateMessage privateMessage)
        {
            foreach(var item in tabItems)
            {
                if(privateMessage.ToId == tabItems[item.Key] || privateMessage.FromId == tabItems[item.Key])
                {

                    if(!listBoxes.FirstOrDefault(x => x.Value == privateMessage.ToId || x.Value == privateMessage.FromId).Key.Items.Contains(privateMessage.dateTime.ToLongDateString()))
                    {
                        listBoxes.FirstOrDefault(x => x.Value == privateMessage.ToId || x.Value == privateMessage.FromId).Key.Items.Add(privateMessage.dateTime.ToLongDateString());
                    }
                    string msg = privateMessage.Msg;
                    if (privateMessage.FromId == ID)
                    {
                        var s = msg.Split(' ');
                        s[1] = "Вы:";
                        msg = "";
                        foreach(var item1 in s)
                        {
                            msg += item1 + " ";
                        }
                    }
                    listBoxes.FirstOrDefault(x => x.Value == privateMessage.ToId || x.Value == privateMessage.FromId).Key.Items.Add(msg);
                    ListBox msgBox = listBoxes.FirstOrDefault(x => x.Value == privateMessage.ToId || x.Value == privateMessage.FromId).Key;
                    msgBox.ScrollIntoView(msgBox.Items[msgBox.Items.Count-1]);
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
            if (item != null && item.SelectedIndex > 0) 
            {
                toID = tabItems[(TabItem)tcChat.Items[tcChat.SelectedIndex]];
               // ListBox msgBox = listBoxes.FirstOrDefault(x => x.Value == toID).Key;
               // msgBox.ScrollIntoView(msgBox.Items[msgBox.Items.Count - 1]);
            }
             
        }

        private void bDisconnect_Click(object sender, RoutedEventArgs e)
        {
            client.Disconnect(ID);
            Chat chat = new Chat();
            chat.Show();
            this.Close();
        }

        public void AllMsgsCallback(PrivateMessage[] allMsgs)
        {
            if (allMsgs.Length == 0)
                return;

            DateTime dateTime = new DateTime();
            foreach (var item in allMsgs)
            {
                

                if(dateTime.ToLongDateString() != item.dateTime.ToLongDateString())
                {
                    dateTime = item.dateTime;
                    listBoxes.FirstOrDefault(x => x.Value == toID).Key.Items.Add(item.dateTime.ToLongDateString());
                }
                
                string msg = item.Msg;
                if (item.FromId == ID)
                {
                    var s = msg.Split(' ');
                    s[1] = "Вы:";
                    msg = "";
                    foreach (var item1 in s)
                    {
                        msg += item1 + " ";
                    }
                }
                listBoxes.FirstOrDefault(x => x.Value == toID).Key.Items.Add(msg);
                ListBox msgBox = listBoxes.FirstOrDefault(x => x.Value == toID).Key;
                msgBox.ScrollIntoView(msgBox.Items[msgBox.Items.Count - 1]);
            }
        }
    }
}
