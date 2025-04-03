using ServerCore;
using System.Net;
using System.Text;
namespace Server
{

    public static class Program
    {
        static LIstener _lIstener = new LIstener();

        public static void Main()
        {
            PacketManager.Instance.Register();

            //DMS == Domain, Name , System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            // CMD  => ping www.google.com


            _lIstener.Init(endpoint, () => { return new ClientSession(); });
            Console.WriteLine("Listening....");

            while (true)
            {
                ;
            }
        }
    }
}