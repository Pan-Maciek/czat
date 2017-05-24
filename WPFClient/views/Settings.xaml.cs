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
using WPFClient.Properties;

namespace WPFClient.views {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl {
        public SettingsView() {
            InitializeComponent();

            IP.Text = Settings.Default.serverLocation;
            port.Text = $"{Settings.Default.serverPort}";

        }

        private void IP_TextChanged(object sender, TextChangedEventArgs e) {
            Settings.Default.serverLocation = IP.Text;
        }

        private void port_TextChanged(object sender, TextChangedEventArgs e) {
            Settings.Default.serverPort = int.Parse("0" + port.Text);
        }
    }
}
