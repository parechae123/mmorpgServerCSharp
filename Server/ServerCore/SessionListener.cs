using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class SessionListener
    {
        static LIstener _lIstener = new LIstener();
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {

                Session session = new Session();
                session.Init(clientSocket); //session에게 clientSocket을 넘겨줌

                //받는다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server");
                session.Send(sendBuff);

                Thread.Sleep(1000);
                session.Disconnect();
                session.Disconnect();

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


            _lIstener.Init(endpoint, OnAcceptHandler);
            Console.WriteLine("Listening....");

            while (true)
            {
                ;
            }
        }
    }
}
