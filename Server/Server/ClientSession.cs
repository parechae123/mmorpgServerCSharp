using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    //클라 => 서버로 연결하는 대리자 : 서버세션
    //서버 => 클라로 연결하는 대리자 : 클라세션 ,방향성을 뜻함
    class Packet
    {
        public ushort size; //사이즈는 음이 아닌 정수이기에 ushort로 선언
        public ushort packetId;
    }
    class PlayerInfoReq : Packet
    {
        public long playerId;
    }
    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    //
    class LoginOkPacket : Packet
    {

    }

    public class ClientSession : PacketSession
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
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset+count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                        count += 8;
                        Console.WriteLine($"Player InfoReq : {playerId}");
                    }
                    break;
                case PacketID.PlayerInfoOk:
                    break;
            }

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
}
