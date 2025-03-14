using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        //비동기 처리를 위한 변수, 1일 시 disConected, 0일 시 connected
        int _disconnected = 0;


        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes) ;
        public abstract void OnDisconnected(EndPoint endPoint) ;

        public void Init(Socket socket)
        {
            _socket = socket;
            
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvComplete);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);
            //recvArgs.UserToken = this;// 식별자 구별,연동을 위해 사용
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(_recvArgs);
            //
        }
        public void Send(byte[] sendBuff)
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

        }
        #region 네트워크 통신

        void RegisterSend()
        {

            while (_sendQueue.Count>0)
            {
                 byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (!pending)
            {
                OnSendCompleted(null, _sendArgs);
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
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
            {
                OnRecvComplete(null, args);
            }
        }

        void OnRecvComplete(object sender,SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
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
