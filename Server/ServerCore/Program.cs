using Microsoft.VisualBasic;
using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Text;

namespace ServerCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SocketServer socketServer = new SocketServer();
            socketServer.Init();
        }
    }

}