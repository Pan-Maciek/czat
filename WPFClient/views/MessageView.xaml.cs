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
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl {

        Client me;

        string userName, targetName;

        public MessageView(string myName, string name, Client me) {
            InitializeComponent();
            this.me = me;
            userName = myName;
            targetName = name;

            label.Content = name;
            me.onMessage += Me_onMessage;
        }

        private void Me_onMessage(object sender, Message message) {
            if (message.From != targetName) return;
            switch (message.type) {
                case MessageType.Message:
                    Dispatcher.Invoke(() => {
                        messages.Children.Add(new UserMessage(message));
                        scroll.ScrollToEnd();
                    });
                    break;
            }
        }

        private void SendClick(object sender, RoutedEventArgs e) {
            string messageContent = new TextRange(messageInput.Document.ContentStart, messageInput.Document.ContentEnd).Text;
            Message message = new Message {
                From = userName,
                To = targetName,
                Content = messageContent,
                type = MessageType.Message
            };
            me.Send(message);
            messages.Children.Add(new UserMessage(message, true));
            scroll.ScrollToEnd();
            messageInput.Document.Blocks.Clear();
        }

        public MessageView() {
            InitializeComponent();
        }
    }
}
