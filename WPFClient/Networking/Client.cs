using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static System.Text.Encoding;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Text;

namespace WPFClient.Networking {
    #region Message

    public enum MessageType {
        Message = 0,
        Protocol = 1,
        Login = 2,
        Users = 3
    }

    public class Message {
        public string To = "", From = "", Content = "";
        public MessageType type = MessageType.Protocol;
        public DateTime date = DateTime.Now;
        public bool succes = true;
    }

    #endregion

    #region Client

    public class Client : IDisposable { // obsługuje pojedynczego klienta
        public static JavaScriptSerializer JSON = new JavaScriptSerializer();
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        StringBuilder sb = new StringBuilder();
        const int BufferSize = 1024;
        byte[] Buffer = new byte[BufferSize];
        
        private void Recive() {
            socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, result => {
                try {
                    int bytesRead = socket.EndReceive(result);
                    if (bytesRead > 0) {
                        sb.Append(UTF8.GetString(Buffer, 0, bytesRead));

                        string content = sb.ToString();
                        if (content.IndexOf("\r\n\r\n;") != -1) {
                            sb.Length -= 5;
                            content = content.Substring(0, content.IndexOf("\r\n\r\n;")).Replace("{{\"", "{").Replace("\"}}", "}");
                            Message message = JSON.Deserialize<Message>(content);
                            onMessage?.Invoke(this, message);
                            Buffer = new byte[BufferSize];
                            sb.Clear();
                        }
                    }
                    Recive();
                } catch (Exception ) {
                    onDisconnect?.Invoke(this, null);
                    MessageBox.Show("Rozłączono z serwera");
                }
            }, null);
        }

        public void Send(Message message) {
            byte[] bytes = UTF8.GetBytes(JSON.Serialize(message) + "\r\n\r\n;");

            try {
                socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, result => {
                    int bytesSend = socket.EndSend(result);
                }, null);
            } catch(Exception) {
                MessageBox.Show("Serwer is disconnected");
            }
        }

        public event EventHandler<Message> onMessage;
        public event EventHandler onDisconnect;

        public void Connect(string destination, int port) {
            IPAddress IP;
            IPAddress.TryParse(destination, out IP);
            if (IP == null) {
                foreach (IPAddress adress in Dns.GetHostAddresses(destination)) {
                    if (adress.AddressFamily == AddressFamily.InterNetwork) {
                        IP = adress;
                        break;
                    }
                }
            }
           
            IPEndPoint endPoint = new IPEndPoint(IP, port);
            socket.BeginConnect(endPoint, result => {
                try {
                    socket.EndConnect(result);
                    Recive();
                } catch (Exception) {
                    MessageBox.Show("Server unavaliable :/");
                }
            }, null);
        }

        public void Dispose() {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    #endregion
}
