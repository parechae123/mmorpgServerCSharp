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
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerID = session.SessionId;
            packet.chat = chat + $"{chat} I am {packet.playerID}";
            ArraySegment<byte> segment = packet.Write();

            //병목현상을 유도하므로 Lock을 사용 할 땐 조심해서 쓰자 일단 얘 자체도 문제임
            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                {
                    s.Send(segment);
                }
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);                
            }
        }
    }
}
