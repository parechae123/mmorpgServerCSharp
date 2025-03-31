using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DummyClient
{
    class PlayerInfoReq
    {
        public long playerId;
        public string name;

        public struct Skill
        {
            public int id;
            public short level;
            public float duration;
            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);

                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);

                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

            }

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);

                return success;
            }

        }

        public List<Skill> skills = new List<Skill>();


        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;
            this.skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);
                skills.Add(skill);
            }

        }
        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.skills.Count);
            count += sizeof(ushort);
            foreach (Skill skill in this.skills)
                success &= skill.Write(s, ref count);
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
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 101, duration = 1.0f, level = 1 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 201, duration = 2.0f, level = 2 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 301, duration = 3.0f, level = 3 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 401, duration = 4.0f, level = 4 });
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
