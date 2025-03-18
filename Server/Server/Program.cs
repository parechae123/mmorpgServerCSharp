using ServerCore;
using System.Net;
using System.Text;
namespace Server
{
    class Packet
    {
        public ushort size; //사이즈는 음이 아닌 정수이기에 ushort로 선언,패킷은 많은양을 주고받기에 메모리 최적화를 위해 Ushort사용
        public ushort packetId;
    }
    //
    class LoginOkPacket : Packet
    {

    }
    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnConnected : {endPoint}");

/*
            Packet packet = new Packet() { size = 4,packetId = 7};

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buffer = BitConverter.GetBytes(packet.size);
            byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

            Send(sendBuff);*/
            Thread.Sleep(5000);
            Disconnect();
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"RecvPacketId: {id}, size {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnDisConnected : {endPoint}");
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