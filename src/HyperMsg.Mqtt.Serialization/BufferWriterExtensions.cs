using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperMsg.Mqtt.Serialization
{
    public static class BufferWriterExtensions
    {
	    private static readonly byte[] Disconnect = { 0b11100000, 0b00000000 };
	    private static readonly byte[] PingResp = { 0b11010000, 0b00000000 };
	    private static readonly byte[] PingReq = { 0b11000000, 0b00000000 };
	    private static readonly byte[] ProtocolName = { (byte)'M', (byte)'Q', (byte)'T', (byte)'T' };

	    const byte ConnectCode = 0b00010000;

	    private static readonly Dictionary<Type, Action<IBufferWriter<byte>, Packet>> writers;

	    static BufferWriterExtensions()
	    {
		    writers = new Dictionary<Type, Action<IBufferWriter<byte>, Packet>>();
		    AddWriter<Connect>(Write);
		    AddWriter<ConnAck>(Write);
		    AddWriter<Publish>(Write);
		    AddWriter<PubAck>(Write);
		    AddWriter<PubRec>(Write);
		    AddWriter<PubRel>(Write);
		    AddWriter<PubComp>(Write);
		    AddWriter<Subscribe>(Write);
		    AddWriter<SubAck>(Write);
		    AddWriter<Unsubscribe>(Write);
		    AddWriter<UnsubAck>(Write);
		    AddWriter<PingReq>(Write);
		    AddWriter<PingResp>(Write);
		    AddWriter<Disconnect>(Write);
		}

	    private static void AddWriter<T>(Action<IBufferWriter<byte>, T> writer) where T : Packet => writers.Add(typeof(T), (w, p) => writer(w, (T)p));

		public static void WriteMqttPacket(this IBufferWriter<byte> writer, Packet packet)
		{
			writers[packet.GetType()](writer, packet);
		}

		private static void Write(IBufferWriter<byte> writer, Connect connect)
		{ }

		private static void Write(IBufferWriter<byte> writer, ConnAck connAck)
		{ }

		private static void Write(IBufferWriter<byte> writer, Publish publish)
		{ }

	    private static void Write(IBufferWriter<byte> writer, PubAck pubAck) => WriteShortPacket(writer, PacketCodes.Puback, pubAck.Id);

	    private static void Write(IBufferWriter<byte> writer, PubRec pubRec) => WriteShortPacket(writer, PacketCodes.Pubrec, pubRec.Id);

	    private static void Write(IBufferWriter<byte> writer, PubRel pubRel) => WriteShortPacket(writer, PacketCodes.Pubrel, pubRel.Id);

		private static void Write(IBufferWriter<byte> writer, PubComp pubComp) => WriteShortPacket(writer, PacketCodes.Pubcomp, pubComp.Id);

		private static void Write(IBufferWriter<byte> writer, Subscribe subscribe)
		{ }

	    private static void Write(IBufferWriter<byte> writer, SubAck subAck)
	    {
	    }

	    private static void Write(IBufferWriter<byte> writer, Unsubscribe unsubscribe)
	    {
		    int length = GetTopicsByteCount(unsubscribe.Topics) + sizeof(ushort) + 4;
		    var span = writer.GetSpan(length);
		    span[0] = PacketCodes.Unsubscribe;
		    //writer.WriteRemainingLength(contentLength);

		    foreach (var topic in unsubscribe.Topics)
		    {
			    //writer.WriteString(topic);
		    }

		    writer.Advance(length);
		}

	    private static int GetTopicsByteCount(string[] topics) => topics.Aggregate(0, (a, s) => a + GetStringByteCount(s));

	    private static int GetStringByteCount(string str) => Encoding.UTF8.GetByteCount(str) + sizeof(ushort);

		private static void Write(IBufferWriter<byte> writer, UnsubAck unsubAck) => WriteShortPacket(writer, PacketCodes.UnsubAck, unsubAck.Id);

	    private static void WriteShortPacket(IBufferWriter<byte> writer, byte code, ushort packetId)
	    {
		    var buffer = writer.GetMemory(4);
		    var span = buffer.Span;

		    span[0] = code;
		    span[1] = 2; //Remaining length always 2
		    BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2), packetId);
		    writer.Advance(4);
	    }

		private static void Write(IBufferWriter<byte> writer, PingReq pingReq) => writer.Write(PingReq);

	    private static void Write(IBufferWriter<byte> writer, PingResp pingResp) => writer.Write(PingResp);

		private static void Write(IBufferWriter<byte> writer, Disconnect disconnect) => writer.Write(Disconnect);

	    public static int WriteRemainingLength(this IBufferWriter<byte> writer, int length)
	    {
		    var span = writer.GetSpan(4);

		    if (length > 0x1fffff)
		    {
			    span[0] = (byte)(length | 0x80);
			    span[1] = (byte)((length >> 7) | 0x80);
			    span[2] = (byte)((length >> 14) | 0x80);
			    span[3] = (byte)(length >> 21);
			    return 4;
		    }

		    if (length > 0x3fff)
		    {
			    span[0] = (byte)(length | 0x80);
			    span[1] = (byte)((length >> 7) | 0x80);
			    span[2] = (byte)(length >> 14);
			    return 3;
		    }

		    if (length > 0x7f)
		    {
			    span[0] = (byte)(length | 0x80);
			    span[1] = (byte)(length >> 7);
			    return 2;
		    }

		    span[0] = (byte)length;
		    return 1;
	    }

	    public static int WriteString(this IBufferWriter<byte> writer, string value)
	    {
		    //var bytes = Encoding.UTF8.GetBytes(value);
		    //buffer.WriteUInt16BigEndian((ushort)bytes.Length, offset);
		    //bytes.CopyTo(buffer.Slice(offset + sizeof(ushort)));
		    //return bytes.Length + sizeof(ushort);
		    throw new NotImplementedException();
	    }
	}
}
