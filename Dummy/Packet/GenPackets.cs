using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Core;

public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	
}

public interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> s);
    ArraySegment<byte> Write();
}


public class C_Chat : IPacket
{
    public string chat;

    public ushort Protocol
    {
        get { return (ushort) PacketID.C_Chat; }
    }

    public void Read(ArraySegment<byte> seg)
    {
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = new ArraySegment<byte>(new byte[4096], 0, 4096);
        Span<byte> s = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        bool success = true;
        ushort count = 0;

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort) PacketID.C_Chat);
        count += sizeof(ushort);
        ushort chatLen = (ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, seg.Array,
		    seg.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
        success &= BitConverter.TryWriteBytes(s.Slice(0, s.Length - count), count);

        if (!success)
            return null;

        return new ArraySegment<byte>(s.ToArray(), 0, count);
    }
}

public class S_Chat : IPacket
{
    public int playerId;
	public string chat;

    public ushort Protocol
    {
        get { return (ushort) PacketID.S_Chat; }
    }

    public void Read(ArraySegment<byte> seg)
    {
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = new ArraySegment<byte>(new byte[4096], 0, 4096);
        Span<byte> s = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        bool success = true;
        ushort count = 0;

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort) PacketID.S_Chat);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		ushort chatLen = (ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, seg.Array,
		    seg.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
        success &= BitConverter.TryWriteBytes(s.Slice(0, s.Length - count), count);

        if (!success)
            return null;

        return new ArraySegment<byte>(s.ToArray(), 0, count);
    }
}

