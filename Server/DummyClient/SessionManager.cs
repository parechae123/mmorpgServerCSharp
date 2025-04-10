﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class SessionManager
    {
        static SessionManager instance = new SessionManager();
        public static SessionManager Instance
        {
            get { return instance; }
        }

        List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();

        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                {
                    C_Chat chatPacket = new C_Chat();
                    chatPacket.chat = $"Hello Server !";
                    ArraySegment<byte> segment = chatPacket.Write();

                    session.Send(segment);
                }
            }
        }

        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                _sessions.Add(session);
                return session;
            }
        }

    }
}
