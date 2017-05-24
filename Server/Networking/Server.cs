using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static System.Text.Encoding;

namespace Messenger.Networking {

    public class Server : IDisposable { // odpowiada za akceptowanie nowych użytkowników

        interface IChatable {
            void Send(Message message);
        }

        #region Message

        static JavaScriptSerializer JSON = new JavaScriptSerializer();

        enum MessageType {
            Message = 0,
            Protocol = 1,
            Login = 2,
            Users = 3
        }

        class Message {
            public string To = "", From = "", Content = "";
            public MessageType type = MessageType.Protocol;
            public DateTime date = DateTime.Now;
            public bool succes = true;
        }

        #endregion

        #region Client

        class Client : IChatable, IDisposable { // obsługuje pojedynczego klienta
            Socket socket;
            StringBuilder sb = new StringBuilder();
            Server server;
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
                                onMessage?.Invoke(this, JSON.Deserialize<Message>(sb.ToString()));
                                Buffer = new byte[BufferSize];
                                sb.Clear();
                            }
                        }
                        Recive();
                    } catch (Exception) {
                        onDisconnect?.Invoke(this, null);
                    }
                }, null);
            }

            public Client(Socket socket, Server s) {
                this.socket = socket;
                server = s;
                Recive();
            }

            public void Send(Message message) {
                byte[] bytes = UTF8.GetBytes(JSON.Serialize(message) + "\r\n\r\n;");

                socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, result => {
                    int bytesSend = socket.EndSend(result);
                }, null);
            }

            public event EventHandler<Message> onMessage;
            public event EventHandler onDisconnect;

            public void Dispose() {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        #endregion

        #region Room

        class Room : IChatable {
            LinkedList<Client> Clients = new LinkedList<Client>();

            public void Send(Message message) {
                foreach (IChatable client in Clients) {
                    client.Send(message);
                }
            }
        }

        #endregion


        #region ServerVaribles

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        Dictionary<string, IChatable> Targets = new Dictionary<string, IChatable>();
        
        #endregion
        
        public void Listen(int port, int backlog = 100) {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            serverSocket.Bind(endPoint);
            serverSocket.Listen(backlog);

            AcceptConnections();
            Console.WriteLine($"Server listening at port: {port}");
        }

        public void Send (string msg) {
            Message message = new Message {
                Content = msg,
                type = MessageType.Message,
                From = "server",
                To = "all"
            };
            foreach (IChatable target in Targets.Values) {
                target.Send(message);
            }
        }

        private void AcceptConnections() {
            serverSocket.BeginAccept(result => {
                AcceptConnections();

                Socket handler = serverSocket.EndAccept(result);
                Client client = new Client(handler, this);
                client.onMessage += (sender, message) => {
                    switch (message.type) {
                        case MessageType.Login:
                            if (Targets.Keys.Contains(message.Content)) {
                                client.Send(new Message {
                                    Content = "User with this name is already loged in",
                                    type = MessageType.Login,
                                    succes = false
                                });
                            } else {
                                Targets.Add(message.Content, client);

                                client.Send(new Message {
                                    type = MessageType.Login,
                                    succes = true
                                });
                                
                                Message newUserMessage = new Message {
                                    type = MessageType.Users,
                                    Content = JSON.Serialize(Targets.Keys.ToArray())
                                };

                                foreach (IChatable _client in Targets.Values) {
                                    if (_client is Client) {
                                        _client.Send(newUserMessage);
                                    }
                                }

                                client.onDisconnect += delegate {
                                    Targets.Remove(message.Content);
                                    Message usersUpdate = new Message {
                                        type = MessageType.Users,
                                        Content = JSON.Serialize(Targets.Keys.ToArray())
                                    };

                                    foreach (IChatable _client in Targets.Values) {
                                        if (_client is Client)
                                            _client.Send(usersUpdate);
                                    }
                                };
                            }
                            break;
                        case MessageType.Message:
                            if (Targets.ContainsKey(message.To)) {
                                Targets[message.To].Send(message);
                            }
                            break;
                        case MessageType.Protocol:
                            break;
                        case MessageType.Users:
                            client.Send(new Message {
                                type = MessageType.Users,
                                Content = JSON.Serialize(Targets.Keys.ToArray())
                            });
                            break;
                    }
                    Console.WriteLine(message.Content);
                };

            }, null);
        }

        public void Dispose() {
           
        }
    }

}
