﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class LIstener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;
        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory,int register = 10, int backLog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            //문지기 교육
            _listenSocket.Bind(endPoint);

            //영업 시작
            //backLog : 최대 대기수
            _listenSocket.Listen(backLog);

            for (int i = 0; i< register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }

        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
                OnAcceptCompleted(null,args);
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Init(args.AcceptSocket); //session에게 clientSocket을 넘겨줌
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            RegisterAccept(args);
        }

        //현재 blocking 형태의 함수이므로 NonBlocking = ex : 비동기 함수 형태로 바꿔줘야함
        public Socket Accept()
        {
            _listenSocket.AcceptAsync();
            return _listenSocket.Accept();
        }
    }
}
