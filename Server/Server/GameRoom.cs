using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
            //멀티쓰래드 환경을 유의하자
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}
