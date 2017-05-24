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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFClient.Networking;

namespace WPFClient.views {
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : UserControl {

        class Converstaion {
            public bool Closed = false;
            public MessageView Panel;
        }

        Converstaion addNewConversation(string with) {

            Button button = new Button();
            Converstaion conv = new Converstaion {
                Panel = new MessageView(userName, with, me)
            };
            
            button.Content = with;
            talks.Children.Add(button);
            button.Click += (sender, e) => {
                conversation.Child = conv.Panel;
            };
            Converstaions.Add(with, conv);

            return conv;
        }

        Client me;

        Dictionary<string, Converstaion> Converstaions = new Dictionary<string, Converstaion>();

        string userName;

        public Chat() {
            InitializeComponent();
        }

        public Chat(Client client, string username) {
            InitializeComponent();
            me = client;
            userName = username;
            me.onMessage += Me_onMessage;
            me.Send(new Message { type = MessageType.Users });
        }

        private void Me_onMessage(object sender, Message message) {
            switch (message.type) {
                case MessageType.Message:
                    Dispatcher.Invoke(() => {
                        if (!Converstaions.ContainsKey(message.From)) {
                            Converstaion conv = addNewConversation(message.From);
                        }
                    });
                    break;
                case MessageType.Protocol:
                    break;
                case MessageType.Users:
                    string[] users = Client.JSON.Deserialize<string[]>(message.Content);
                    Dispatcher.Invoke(() => {
                        this.users.Children.Clear();
                        foreach (string user in users) {
                            if (user == userName) continue;
                            Button b = new Button();
                            b.Click += ActiveUserClick;
                            b.Content = user;
                            this.users.Children.Add(b);
                        }
                    });
                    break;
            }
        }

        private void ActiveUserClick(object sender, RoutedEventArgs e) {
            string selectedUsernName = ((Button)sender).Content as string;
            Converstaion conv;
            if (Converstaions.ContainsKey(selectedUsernName)) {
                conv = Converstaions[selectedUsernName];
            } else {
                conv = addNewConversation(selectedUsernName);
                me.Send(new Message {
                    To = selectedUsernName,
                    type = MessageType.Message,
                    From = userName 
                });
            }
            conversation.Child = conv.Panel;

        }
    }
}
