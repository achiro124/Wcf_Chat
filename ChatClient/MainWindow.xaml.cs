﻿using System;
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
        private List<TabItem> tabItems = new List<TabItem>();
        private List<ListBox> listBoxes = new List<ListBox>();
        private List<TextBox> textBoxes = new List<TextBox>();
        private Dictionary<int,string> listUsers = new Dictionary<int,string>();

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
                if (item.Header.ToString() == lbUsers.Items[lbUsers.SelectedIndex].ToString())
                {
                    tcChat.SelectedIndex = tabItems.IndexOf(item) + 1;
                   // lbUsers.SelectionChanged -= lbUsers_SelectionChanged;
                   // lbUsers.SelectedIndex = -1;
                   // lbUsers.SelectionChanged += lbUsers_SelectionChanged;
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

            listBoxes.Add(chatbox);
            textBoxes.Add(textBox);

            TabItem tabItem = new TabItem
            {
                Header = lbUsers.Items[lbUsers.SelectedIndex].ToString(),
                Height = 30,
                MinWidth = 80,
                Content = grid,
                Name = "tabItem" + listUsers.FirstOrDefault(x => x.Value == lbUsers.Items[lbUsers.SelectedIndex].ToString()).Key.ToString()
            };
            tabItems.Add(tabItem);
            tcChat.Items.Add(tabItem);
            tcChat.SelectedIndex = tcChat.Items.Count - 1;


            toID = listUsers.FirstOrDefault(x => x.Value == lbUsers.Items[lbUsers.SelectedIndex].ToString()).Key;
         //  lbUsers.SelectionChanged -= lbUsers_SelectionChanged;
         //  lbUsers.SelectedIndex = -1;
         //  lbUsers.SelectionChanged += lbUsers_SelectionChanged;
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
                    if (textBox.Name == item.Name)
                    {

                        if (client != null)
                        {
                            client.SendMsg(item.Text, ID, toID);
                            item.Text = string.Empty;
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
            for(int i = 0; i < tabItems.Count; i++)
            {
                if(ToId.ToString() == tabItems[i].Name.Remove(0,7) || fromId.ToString() == tabItems[i].Name.Remove(0, 7))
                {
                    listBoxes[i].Items.Add(msg);
                    //listBoxes[i].ScrollIntoView(listBoxes[i].Items[listBoxes[i].Items.Count - 1]);
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

        public void AllMsgsCallback(PrivateMessage[] allMsgs)
        {
            for (int i = 0; i < tabItems.Count; i++)
            {
                if (toID.ToString() == tabItems[i].Name.Remove(0, 7) || ID.ToString() == tabItems[i].Name.Remove(0, 7))
                {
                    foreach(var item in allMsgs)
                    {
                        listBoxes[i].Items.Add(item.Text);
                    }
                    break;
                }
            }
        }
    }
}
