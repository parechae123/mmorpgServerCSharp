using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DummyClient
{

    public class ServerSession : PacketSession
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
        }

        public override void OnDisconnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this,buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes : {numOfBytes}");

        }
    }
}
