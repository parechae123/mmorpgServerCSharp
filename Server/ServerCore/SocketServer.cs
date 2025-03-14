using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class SocketServer
    {
        static LIstener _lIstener = new LIstener();
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                //받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);

                Console.WriteLine($"[FromClient] {recvData}");

                //보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom To Server");
                clientSocket.Send(sendBuff);

                //쫒겨난다
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        public void Init()
        {
            //DMS == Domain, Name , System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            // CMD  => ping www.google.com


            //_lIstener.Init(endpoint,OnAcceptHandler);
            Console.WriteLine("Listening....");

            while (true)
            {
                ;
            }
        }
    }
}
