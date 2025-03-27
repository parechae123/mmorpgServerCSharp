using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Packet
    {
        //packet의 header변수들
        //클라이언트에서 임의로 수정이 가능한 영역이므로 참고만 해야함
        public ushort size; //사이즈는 음이 아닌 정수이기에 ushort로 선언
        public ushort packetId;


        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }
    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name; //가변적인 크기의 대표적인 데이터

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="s">데이터 유효범위</param>
            /// <param name=""></param>
            /// <returns></returns>
            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);

                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();//TODO : 가변크기의 패킷유형


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
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            //write 의 지역번수 nameLen을 추출하는 과정

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            //byte=>string
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            //skill list
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);
            }



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


            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            //skillList
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (SkillInfo skill in skills)
            {
                success &= skill.Write(s, ref count);
            }

            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false) return null;


            return SendBufferHelper.Close(count);
        }
    }
    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;

        public override void Read(ArraySegment<byte> s)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Write()
        {
            throw new NotImplementedException();
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    //
    class LoginOkPacket : Packet
    {
        public override void Read(ArraySegment<byte> s)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Write()
        {
            throw new NotImplementedException();
        }
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

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);

                        Console.WriteLine($"Player InfoReq : {p.playerId} {p.name}");
                        foreach (PlayerInfoReq.SkillInfo skill in p.skills)
                        {
                            Console.WriteLine($"({skill.id})({skill.level})({skill.duration})");
                        }
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
