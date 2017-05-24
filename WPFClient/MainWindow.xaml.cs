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
using WPFClient.views;
using WPFClient.Networking;
using WPFClient.Properties;

namespace WPFClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        LoginView LoginScreen = new LoginView();
        SettingsView SettingsScreen = new SettingsView();
        ContentControl back;
        Client me;

        public MainWindow() {
            InitializeComponent();
            grid.Children.Add(LoginScreen);
            
            LoginScreen.settings.Click += delegate {
                grid.Children.Add(SettingsScreen);
                grid.Children.Remove(LoginScreen);
                back = LoginScreen;
            };

            SettingsScreen.back.Click += delegate {
                grid.Children.Remove(SettingsScreen);
                grid.Children.Add(LoginScreen);
            };

            LoginScreen.join.Click += delegate {
                if (LoginScreen.user.Text.Length > 3) {
                    LoginScreen.join.IsEnabled = false;
                    me = new Client();
                    me.onMessage += Me_onMessage;
                    me.Connect(Settings.Default.serverLocation, Settings.Default.serverPort);
                    me.Send(new Message {
                        Content = LoginScreen.user.Text,
                        type = MessageType.Login
                    });
                } else {
                    MessageBox.Show("User name is too short");
                }
            };

        }


        private void Me_onMessage (object sender, Message message) {
            switch (message.type) {
                case MessageType.Login:
                    if (message.succes) {
                        Dispatcher.Invoke(() => {
                            grid.Children.Clear();
                            Chat chat = new Chat(me, LoginScreen.user.Text);
                            grid.Children.Add(chat);
                        });
                    } else MessageBox.Show(message.Content);
                    break;
            }
        }
    }
}
