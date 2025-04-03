﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
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
            PacketManager.Instance.OnRecvPacket(this,buffer);
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
