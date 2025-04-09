using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int headerSize = 2;

        //[size(2)][packet(2)][....(종속데이터)]
        /// <summary>
        /// sealed 키워드는 해당 클래스가 부모클래스일 시 자식 클래스에서 
        /// override할 수 없게끔 봉인함
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            
            while (true)
            {

                //packet 클래스의 size,packetId는 각각 2바이트 이므로 buffer가 최소한 2byte 보다 커야함
                //최소한 header를 파싱 할 수 있는 지 판단
                if (buffer.Count < headerSize)
                {
                    break;
                }
                //패킷 손실이 없는지 확인 
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);//버퍼 array를 조사하여 offsset부터 데이터가 있는 값들을 반환
                if (buffer.Count < dataSize)
                {
                    break;
                }
                OnRecvPacket(new ArraySegment<byte>(buffer.Array,buffer.Offset,dataSize));//패킷의 유효범위

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset+dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }
    public abstract class Session
    {
        Socket _socket;
        //비동기 처리를 위한 변수, 1일 시 disConected, 0일 시 connected
        int _disconnected = 0;

        ReceiveBuffer _recvBuff = new ReceiveBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes) ;
        public abstract void OnDisconnected(EndPoint endPoint) ;

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Init(Socket socket)
        {
            _socket = socket;
            
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvComplete);
            
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(_recvArgs);
            //
        }
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                //_socket.Send(sendBuff);
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)//데이터가 없을 때 만 보내기 위해
                    RegisterSend();
            }
        }
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;
            
            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();

        }
        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnected == 1)
                return;


            while (_sendQueue.Count>0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }

            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (!pending)
                {
                    OnSendCompleted(null, _sendArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSendFailed : {e}");
                throw;
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }

                    }
                    catch (Exception e)
                    {

                        Console.WriteLine($"OnSendCompleted Failed : {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }

        }

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            if (_disconnected == 1)
                return;

            _recvBuff.Clean();
            ArraySegment<byte> segment = _recvBuff.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(args);
                if (pending == false)
                {
                    OnRecvComplete(null, args);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed : {e}");
            }

        }

        void OnRecvComplete(object sender,SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write 커서 이동
                    if (_recvBuff.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }
                    //컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다



                    int processLen = OnRecv(_recvBuff.ReadSegment);
                    if (processLen< 0 || _recvBuff.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    //read 커서 이동
                    if (_recvBuff.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"On RecvCompleted Failed{e}");
                }
            }

            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
