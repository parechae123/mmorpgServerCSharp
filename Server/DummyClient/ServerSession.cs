using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DummyClient
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

    public class ServerSession : Session
    {
/*      C#에서 사용 하는 포인터 예제
        //unsafe 키워드를 넣을 시 C++의 포인터마냥 쓸 수 있음
        /// <summary>
        /// unsafe 사용 예제
        /// </summary>
        /// <param name="array">타겟 array</param>
        /// <param name="offset">value를 넣을 위치</param>
        /// <param name="value">offset 위치에 넣을 값</param>
        static unsafe void ToBytes(byte[] array, int offset,ulong value)
        {
            //* == 역참조 연산자(주소의 값을 가져올 때,수정할때
            //& == 주소 연산자(특정 변수의 주소를 가져올 때 사용)
            //ptr 
            fixed (byte* ptr = &array[offset])
                *(ulong*)ptr = value;
        }
*/

        public override void OnConnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnConnected : {endPoint}");

            //구성요소의 Ushort는 16bit, 1 byte == 8bit 이기에 1 ushort = size 2
            //2ushort == 4byte == size == 4
            PlayerInfoReq packet = new PlayerInfoReq() { packetId = (ushort)PacketID.PlayerInfoReq,playerId = 1001 };

            //보낸다
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = SendBufferHelper.Open(4096);         //openSegment

                //packet의 쓰기 위치를 갱신 하기 위해 지역변수 count로
                //이전 배열의 size를 감안하여 자동화를 위한 지역변수
                ushort count = 0;
                bool success = true;

                //and equal 연산자 == "success&BitConverter.TryWriteBytes.... 와 같음
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count),packet.packetId);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array,s.Offset + count, s.Count - count),packet.playerId);
                count += 8;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array,s.Offset, s.Count),count);

                ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

                //받은 패킷의 누락점이 없을 시
                if (success)
                {
                    //패킷을 서버로 보낸다
                    Send(sendBuff);
                }

            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");

        }
    }
}
