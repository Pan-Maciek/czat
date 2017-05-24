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
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace WPFClient.views {

    /// <summary>
    /// Interaction logic for UserMessage.xaml
    /// </summary>
    public partial class UserMessage : UserControl {

        private ImageSource resourceToSource(System.Drawing.Bitmap b) {

            MemoryStream ms = new MemoryStream();
            b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        private InlineUIContainer imgFromSource(System.Drawing.Bitmap b, int w, int h) {
            Image img = new Image();
            InlineUIContainer inline = new InlineUIContainer();

            img.Source = resourceToSource(b);
            img.Height = h;
            img.Width = w;
            
            inline.Child = img;
            return inline;
        }

        public UserMessage(Message message, bool local = false) {
            InitializeComponent();

            string[] fragments = Regex.Split(message.Content.Trim() + ' ', @"(O:\)|:D|;\)|<3|:\))");
            Paragraph p = new Paragraph();
            foreach (string fragment in fragments) {
                switch (fragment) {
                    case "O:)":
                        p.Inlines.Add(imgFromSource(Properties.Resources.A, 16, 16));
                        break;
                    case ":D":
                        p.Inlines.Add(imgFromSource(Properties.Resources.D, 16, 16));
                        break;
                    case ";)":
                        p.Inlines.Add(imgFromSource(Properties.Resources.E, 16, 16));
                        break;
                    case "<3":
                        p.Inlines.Add(imgFromSource(Properties.Resources.H, 16, 16));
                        break;
                    case ":)":
                        p.Inlines.Add(imgFromSource(Properties.Resources.S, 16, 16));
                        break;
                    default:
                        Run run = new Run();
                        run.Text = fragment.Trim();
                        p.Inlines.Add(run);
                        break;
                }
            }
            this.message.Document.Blocks.Clear();
            this.message.Document.Blocks.Add(p);
            this.message.IsReadOnly = true;
            date.Content = $"{message.date.ToLongDateString()} {message.date.ToShortTimeString()}";
            if (local) {
                Margin = new Thickness(25, 5, 75, 5);
                date.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        public UserMessage() {
            InitializeComponent();
        }
    }
}
