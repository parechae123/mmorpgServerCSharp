using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace DummyClient
{


    class Program
    {
        static void Main(string[] args)
        {
            //DMS == Domain, Name , System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);
            Console.WriteLine("여기");
            Connector connector = new Connector();

            connector.Connect(endpoint, () => { return new ServerSession(); });
            while (true)
            {
                try
                {
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(100);
            }

            

        }
    }
}