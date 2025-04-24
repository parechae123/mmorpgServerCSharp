using ServerCore;
using System.Net;
using System.Text;
namespace Server
{

    public static class Program
    {
        static LIstener _lIstener = new LIstener();
        public static GameRoom Room = new GameRoom();
        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250);
        }
        public static void Main()
        {

            //DMS == Domain, Name , System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            // CMD  => ping www.google.com


            _lIstener.Init(endpoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening....");

            int roomTick = 0;

            //FlushRoom();
            JobTimer.Instance.Push(FlushRoom);

            while (true)
            {
                JobTimer.Instance.Flush();

                //tickCount
            }
        }
    }
}