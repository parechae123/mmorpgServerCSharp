using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DummyClient
{
    //클라 => 서버로 연결하는 대리자 : 서버세션
    //서버 => 클라로 연결하는 대리자 : 클라세션 ,방향성을 뜻함
    public abstract class Packet
    {
        //packet의 header변수들
        //클라이언트에서 임의로 수정이 가능한 영역이므로 참고만 해야함
        public ushort size; //사이즈는 음이 아닌 정수이기에 ushort로 선언
        public ushort packetId;

        public List<int> skills = new List<int>();//TODO : 가변크기의 패킷유형

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }
    class PlayerInfoReq : Packet 
    {
        public long playerId;
        public string name; //가변적인 크기의 대표적인 데이터

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        //[][][][][][][][][][][][] 12바이트
        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;
            
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);
            //읽기만 가능한 숫자값으로 playerID로 대체
            //현재까지의 count == header 영역의 배열이기에 offset+count 하여 자식 클래스의 변수 부터
            //s.count-count == 자식클래스의 데이터 영역까지만 가져와서 값을 지정
            this.playerId = BitConverter.ToInt64(s.Slice(count,s.Length-count));
            count += sizeof(long);

            //write 의 지역번수 nameLen을 추출하는 과정

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            //byte=>string
            this.name = Encoding.Unicode.GetString(s.Slice(count,nameLen));
        }
        /// <summary>
        /// packet 자식 클래스 내에서 Serialize 하는 함수
        /// </summary>
        /// <returns></returns>
        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);         //openSegment

            //packet의 쓰기 위치를 갱신 하기 위해 지역변수 count로
            //이전 배열의 size를 감안하여 자동화를 위한 지역변수
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);
/*            success &= BitConverter.TryWriteBytes(s, count);

            //string
            //string len [2]
            //byte[]

            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            success &= BitConverter.TryWriteBytes(s.Slice(count,s.Length-count),nameLen);
            count += sizeof(ushort);
            //EX : Array.Copy(i, 1, segmentArray, 0, i.Length);
            //라고 가정 했을 시 segmentArray에 0번째부터 i배열을 1~i.Length까지 카피해온다는 뜻

            Array.Copy(Encoding.Unicode.GetBytes(this.name),0,segment.Array,count,nameLen);
            count += nameLen;*/


            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count+sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false) return null;


            return SendBufferHelper.Close(count);
        }
    }
/*
    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }
*/
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }
/*
    //
    class LoginOkPacket : Packet
    {

    }*/

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
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001,name = "ABCD" };

            //보낸다
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = packet.Write();

                //받은 패킷의 누락점이 없을 시
                if (s != null)
                    Send(s);//패킷을 서버로 보낸다

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
