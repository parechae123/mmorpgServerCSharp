
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
    C_PlayerInfoReq = 1,
	S_Test = 2,
	C_Chat = 3,
	S_Chat = 4,
	
}

interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}


class C_PlayerInfoReq : IPacket
{
    public byte testByte;
	public long playerId;
	public string name;
	
	public List<Skill> skills = new List<Skill>();
	
	public class Skill
	{
	    public int id;
		public short level;
		public float duration;
		
		public List<Attribute> attributes = new List<Attribute>();
		
		public class Attribute
		{
		    public int att;
			public int attTwo;
			public int attThree;
		    public void Read(ArraySegment<byte> s ,ref ushort count)
		    {
		        this.att = BitConverter.ToInt32(segment.Array, segment.Offset + count);
				count += sizeof(int);
				
				this.attTwo = BitConverter.ToInt32(segment.Array, segment.Offset + count);
				count += sizeof(int);
				
				this.attThree = BitConverter.ToInt32(segment.Array, segment.Offset + count);
				count += sizeof(int);
				
		    }
		
		    public bool Write(ArraySegment<byte> s,ref ushort count)
		    {
		        bool success = true;
		        Array.Copy(BitConverter.GetBytes(this.att), 0, segment.Array, segment.Offset + count, sizeof(int));
				count += sizeof(int);
				Array.Copy(BitConverter.GetBytes(this.attTwo), 0, segment.Array, segment.Offset + count, sizeof(int));
				count += sizeof(int);
				Array.Copy(BitConverter.GetBytes(this.attThree), 0, segment.Array, segment.Offset + count, sizeof(int));
				count += sizeof(int);
		
		        return success;
		    }
		
		}
		
		
	    public void Read(ArraySegment<byte> s ,ref ushort count)
	    {
	        this.id = BitConverter.ToInt32(segment.Array, segment.Offset + count);
			count += sizeof(int);
			
			this.level = BitConverter.ToInt16(segment.Array, segment.Offset + count);
			count += sizeof(short);
			
			this.duration = BitConverter.ToSingle(segment.Array, segment.Offset + count);
			count += sizeof(float);
			
			this.attributes.Clear();
			ushort attributeLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count));
			count += sizeof(ushort);
			
			for (int i = 0; i< attributeLen; i++)
			{
			    Attribute attribute = new Attribute();
			    attribute.Read(s, ref count);
			    attributes.Add(attribute);
			}
			
	    }
	
	    public bool Write(ArraySegment<byte> s,ref ushort count)
	    {
	        bool success = true;
	        Array.Copy(BitConverter.GetBytes(this.id), 0, segment.Array, segment.Offset + count, sizeof(int));
			count += sizeof(int);
			Array.Copy(BitConverter.GetBytes(this.level), 0, segment.Array, segment.Offset + count, sizeof(short));
			count += sizeof(short);
			Array.Copy(BitConverter.GetBytes(this.duration), 0, segment.Array, segment.Offset + count, sizeof(float));
			count += sizeof(float);
			Array.Copy(BitConverter.GetBytes((ushort)this.attributes.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
			count += sizeof(ushort);
			foreach (Attribute attribute in this.attributes)
			    attribute.Write(segment, ref count);
	
	        return success;
	    }
	
	}
	
	
	public ushort Protocol { get { return (ushort)PacketID.C_PlayerInfoReq; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        this.testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
		this.playerId = BitConverter.ToInt64(segment.Array, segment.Offset + count);
		count += sizeof(long);
		
		ushort nameLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(segment.Array, segment.Offset + count,nameLen);
		count += nameLen;
		this.skills.Clear();
		ushort skillLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count));
		count += sizeof(ushort);
		
		for (int i = 0; i< skillLen; i++)
		{
		    Skill skill = new Skill();
		    skill.Read(s, ref count);
		    skills.Add(skill);
		}
		
    }
    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_PlayerInfoReq), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        segment.Array[segment.Offset + count] = (byte)this.testByte;
		count += sizeof(byte);
		Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(long));
		count += sizeof(long);
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count+sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(this.nameLen), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += nameLen;
		Array.Copy(BitConverter.GetBytes((ushort)this.skills.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		foreach (Skill skill in this.skills)
		    skill.Write(segment, ref count);

		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

class S_Test : IPacket
{
    public int testInt;
	public string name;
	public ushort Protocol { get { return (ushort)PacketID.S_Test; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        this.testInt = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		count += sizeof(int);
		
		ushort nameLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(segment.Array, segment.Offset + count,nameLen);
		count += nameLen;
    }
    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_Test), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes(this.testInt), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count+sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(this.nameLen), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += nameLen;

		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

class C_Chat : IPacket
{
    public string chat;
	public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        ushort chatLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count,chatLen);
		count += chatLen;
    }
    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_Chat), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count+sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(this.chatLen), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += chatLen;

		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

class S_Chat : IPacket
{
    public int playerID;
	public string chat;
	public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerID = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		count += sizeof(int);
		
		ushort chatLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count,chatLen);
		count += chatLen;
    }
    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_Chat), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes(this.playerID), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count+sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(this.chatLen), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += chatLen;

		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

