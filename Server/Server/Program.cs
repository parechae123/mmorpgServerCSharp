using ServerCore;
using System.Net;
using System.Text;
namespace Server
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnConnected : {endPoint}");
            //받는다
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server");
            Send(sendBuff);

            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");

        }
    }
    public static class Program
    {
        static LIstener _lIstener = new LIstener();

        public static void Main()
        {
            //DMS == Domain, Name , System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            // CMD  => ping www.google.com


            _lIstener.Init(endpoint, () => { return new GameSession(); });
            Console.WriteLine("Listening....");

            while (true)
            {
                ;
            }
        }
    }
}