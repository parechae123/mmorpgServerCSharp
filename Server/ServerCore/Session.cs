using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        //비동기 처리를 위한 변수, 1일 시 disConected, 0일 시 connected
        int _disconnected = 0;

        public void Init(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvComplete);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);
            //recvArgs.UserToken = this;// 식별자 구별,연동을 위해 사용

            RegisterRecv(recvArgs);
            //
        }
        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
        }
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

        }
        #region 네트워크 통신

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
                //TODO 
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"On RecvCompleted Failed{e}");
                    throw;
                }
            }

            else
            {
                Console.WriteLine($"[SocketError] : {args.SocketError}");
                //TODO : DIsconnect
            }
        }
        #endregion
    }
}
