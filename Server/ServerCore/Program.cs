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
            //DMS == Domain, Name , System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            // CMD  => ping www.google.com
            Socket listenSocket = new Socket(endpoint.AddressFamily,SocketType.Stream,ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(endpoint);

                //backLog = 최대 대기수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Listening....");

                    //손님을 입장시킨다
                    Socket clientSocket = listenSocket.Accept();

                    //받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);

                    Console.WriteLine($"[FromClient] {recvData}");

                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom To Server");
                    clientSocket.Send(sendBuff);

                    //보낸다
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }

}