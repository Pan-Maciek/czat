using Messenger.Networking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Text.Encoding;

class Program {
    public static int Main(string[] args) {

        Server server = new Server();
        server.Listen(7777);

        while (true) {
            string msg = Console.ReadLine();
            server.Send(msg);
        }
    }
    
}