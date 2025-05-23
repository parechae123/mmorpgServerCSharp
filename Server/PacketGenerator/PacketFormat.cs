﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    class PacketFormat
    {
        //{0} 패킷 등록
        public static string managerFormat =
@"using System;
using ServerCore;
using System.Generics.Collection;
class PacketManager
{{
    #region Singleton
    private static PacketManager _instance = new PacketManager();
    public static PacketManager Instance{{ get {{ return _instance; }}}}
    #endregion

    PacketManager()
    {{
        Register();
    }}

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();


    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id,out action))
            action.Invoke(session, buffer);
    }}
    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T pkt = new T();
        pkt.Read(buffer);
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol,out action))
            action.Invoke(session, pkt);
    }}
}}
";
        //{0} 패킷 이름
        public static string managerRegisterFormat =
@"
        _onRecv.Add((ushort)PacketID.{0},MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);
";

        //{0} 패킷 이름들이 들어갈 자리
        //{1} 패킷 목록
        public static string fileFormat =
@"
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{{
    {0}
}}

interface IPacket
{{
    ushort Protocol {{ get; }}
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}}

{1}
";
        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";


        //{0} 패킷 이름
        //{1} 멤버 변수들
        //{2} 멤버 변수 read
        //{3} 멤버 변수 write

        public static string packetFormat =
        @"
class {0} : IPacket
{{
    {1}
	public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}
    }}
    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.{0}), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        {3}

		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }}
}}
";
        //{0} 변수 형식
        //{1} To~ 변수형식
        //{2} 변수형식
        public static string memberFormat =
@"public {0} {1};";

        //{0} 리스트 이름 [파스칼케이스]
        //{1} 리스트 이름 [소문자]
        //{2} 멤버 변수들
        //{3} 멤버 변수들 read
        //{4} 멤버 변수들 write
        public static string memberListFormat =
@"
public List<{0}> {1}s = new List<{0}>();

public class {0}
{{
    {2}
    public void Read(ArraySegment<byte> s ,ref ushort count)
    {{
        {3}
    }}

    public bool Write(ArraySegment<byte> s,ref ushort count)
    {{
        bool success = true;
        {4}

        return success;
    }}

}}

";

        public static string readFormat =
@"this.{0} = BitConverter.{1}(segment.Array, segment.Offset + count);
count += sizeof({2});
";

        //{0} 변수명
        //{1} ubyte byte 캐스팅을 위한 변수형식
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";


        //{0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(segment.Array, segment.Offset + count,{0}Len);
count += {0}Len;";

        //{0} 리스트 이름 {대문자}
        //{1} 리스트 이름 {소문자}
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(segment.Array, segment.Offset + count));
count += sizeof(ushort);

for (int i = 0; i< {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}
";

        //{0} 변수 이름
        //{1} 변수 형식
        public static string writeFormat =
@"Array.Copy(BitConverter.GetBytes(this.{0}), 0, segment.Array, segment.Offset + count, sizeof({1}));
count += sizeof({1});";
        //{0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count+sizeof(ushort));
Array.Copy(BitConverter.GetBytes(this.{0}Len), 0, segment.Array, segment.Offset + count, sizeof(ushort));
count += sizeof(ushort);
count += {0}Len;";

        //{0} 리스트 이름 [대문자]
        //{1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"Array.Copy(BitConverter.GetBytes((ushort)this.{1}s.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
    {1}.Write(segment, ref count);";
        //{0} 변수명
        //{1} ubyte byte 캐스팅을 위한 변수형식
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = ({1})this.{0};
count += sizeof({1});";
    }
}
