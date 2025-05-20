using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{

    internal static void S_TestHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket =packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        //if (chatPacket.playerID == 1)
        //Console.WriteLine(chatPacket.chat);
    }
}
