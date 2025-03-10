using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        public static void Main(string[] args)
        {
            //DMS == Domain, Name , System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            while (true)
            {
                //휴대폰 설정
                Socket socket = new Socket(endpoint.AddressFamily,SocketType.Stream,ProtocolType.Tcp);

                try
                {
                    //문지기에게 입장문의
                    socket.Connect(endpoint);
                    Console.WriteLine($"Connect To {socket.RemoteEndPoint.ToString()}");
                    //보낸다
                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World!{i}");
                        int sendBytes = socket.Send(sendBuff);
                    }


                    //받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    //나간다
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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