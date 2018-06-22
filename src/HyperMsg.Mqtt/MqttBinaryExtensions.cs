using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperMsg.Mqtt
{
	public static class MqttBinaryExtensions
	{
		private static readonly byte[] Disconnect = { 0b11100000, 0b00000000 };
		private static readonly byte[] PingResp = { 0b11010000, 0b00000000 };
		private static readonly byte[] PingReq = { 0b11000000, 0b00000000 };

		const byte ConnectCode = 0b00010000;

		private static readonly Dictionary<Type, Func<Memory<byte>, Packet, int>> writers;

		static MqttBinaryExtensions()
		{
			writers = new Dictionary<Type, Func<Memory<byte>, Packet, int>>();
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

		public static (Packet packet, int size) ReadPacket(this ReadOnlyMemory<byte> buffer)
		{
			throw new NotImplementedException();
		}

		public static int ReadRemainingLength(this ReadOnlyMemory<byte> buffer)
		{
			throw new NotImplementedException();
		}

		public static string ReadString(this ReadOnlyMemory<byte> buffer)
		{
			throw new NotImplementedException();
		}

		public static int WritePacket(this Memory<byte> buffer, Packet packet) => writers[packet.GetType()](buffer, packet);

		private static int Write(Memory<byte> buffer, PubAck packet) => WriteShortPacket(buffer, PacketCodes.Puback, packet.Id);

		private static int Write(Memory<byte> buffer, PubRec packet) => WriteShortPacket(buffer, PacketCodes.Pubrec, packet.Id);

		private static int Write(Memory<byte> buffer, PubRel packet) => WriteShortPacket(buffer, PacketCodes.Pubrel, packet.Id);

		private static int Write(Memory<byte> buffer, PubComp packet) => WriteShortPacket(buffer, PacketCodes.Pubcomp, packet.Id);

		private static int Write(Memory<byte> buffer, Subscribe packet)
		{
			var span = buffer.Span;

			span[0] = PacketCodes.Subscribe;

			int contentLength = GetSubscriptionsByteCount(packet.Subscriptions) + sizeof(ushort);
			int offset = buffer.WriteRemainingLength(contentLength, 1) + 1;
			buffer.WriteUInt16BigEndian(packet.Id, offset);
			offset += sizeof(ushort);

			foreach (var (topic, qos) in packet.Subscriptions)
			{
				offset += buffer.WriteString(topic, offset);
				span[offset] = (byte)qos;
				offset++;
			}

			return offset;
		}

		private static int GetSubscriptionsByteCount((string, QosLevel)[] subscriptions) => subscriptions.Aggregate(0, (a, s) => a + GetStringByteCount(s.Item1) + 1);

		private static int Write(Memory<byte> buffer, SubAck packet)
		{
			var span = buffer.Span;

			span[0] = PacketCodes.SubAck;

			int contentLength = packet.Results.Length + sizeof(ushort);
			int offset = buffer.WriteRemainingLength(contentLength, 1) + 1;
			buffer.WriteUInt16BigEndian(packet.Id, offset);
			offset += sizeof(ushort);

			foreach (var result in packet.Results)
			{
				span[offset] = (byte)result;
				offset++;
			}

			return offset;
		}

		private static int Write(Memory<byte> buffer, Unsubscribe packet)
		{
			var span = buffer.Span;

			span[0] = PacketCodes.Unsubscribe;
			var offset = 1;
			var contentLength = GetTopicsByteCount(packet.Topics) + sizeof(ushort);
			offset += buffer.WriteRemainingLength(contentLength, offset);

			buffer.WriteUInt16BigEndian(packet.Id, offset);
			offset += sizeof(ushort);

			foreach (var filter in packet.Topics)
			{
				offset += buffer.WriteString(filter, offset);
			}

			return offset;
		}

		private static int GetTopicsByteCount(string[] topics) => topics.Aggregate(0, (a, s) => a + GetStringByteCount(s));

		private static int GetStringByteCount(string str) => Encoding.UTF8.GetByteCount(str) + sizeof(ushort);

		private static int Write(Memory<byte> buffer, UnsubAck packet) => WriteShortPacket(buffer, PacketCodes.UnsubAck, packet.Id);

		private static int WriteShortPacket(Memory<byte> buffer, byte code, ushort packetId)
		{
			var span = buffer.Span;

			span[0] = code;
			span[1] = 2; //Remaining length always 2
			buffer.WriteUInt16BigEndian(packetId, 2);
			return 4;
		}

		private static int Write(Memory<byte> buffer, PingReq packet)
		{
			PingReq.CopyTo(buffer);
			return PingReq.Length;
		}

		private static int Write(Memory<byte> buffer, PingResp packet)
		{
			PingResp.CopyTo(buffer);
			return PingResp.Length;
		}

		private static int Write(Memory<byte> buffer, Disconnect packet)
		{
			Disconnect.CopyTo(buffer);
			return Disconnect.Length;
		}

		public static int WriteRemainingLength(this Memory<byte> buffer, int length, int offset)
		{
			var span = buffer.Span;

			if (length > 0x1fffff)
			{
				span[offset] = (byte)(length | 0x80);
				span[offset + 1] = (byte)((length >> 7) | 0x80);
				span[offset + 2] = (byte)((length >> 14) | 0x80);
				span[offset + 3] = (byte)(length >> 21);
				return 4;
			}

			if (length > 0x3fff)
			{
				span[0] = (byte)(length | 0x80);
				span[0 + 1] = (byte)((length >> 7) | 0x80);
				span[0 + 2] = (byte)(length >> 14);
				return 3;
			}

			if (length > 0x7f)
			{
				span[0] = (byte)(length | 0x80);
				span[0 + 1] = (byte)(length >> 7);
				return 2;
			}

			span[offset] = (byte) length;
			return 1;
		}

		public static int WriteString(this Memory<byte> buffer, string value, int offset)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			buffer.WriteUInt16BigEndian((ushort)bytes.Length, offset);
			bytes.CopyTo(buffer.Slice(offset + sizeof(ushort)));
			return bytes.Length + sizeof(ushort);
		}

		private static void AddWriter<T>(Func<Memory<byte>, T, int> writer) where T : Packet => writers.Add(typeof(T), (w, p) => writer(w, (T)p));
	}
}