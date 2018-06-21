using System;
using System.Collections.Generic;

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

		public static int WritePacket(this Memory<byte> buffer, Packet packet)
		{
			return writers[packet.GetType()](buffer, packet);
		}

		private static int Write(Memory<byte> buffer, Unsubscribe packet)
		{
			var span = buffer.Span;
			buffer.WriteUInt16BigEndian(packet.Id, 0);

			//foreach (var filter in packet.Topics)
			//{
			//	buffer.WriteString(filter);
			//}

			span[0] = PacketCodes.Unsubscribe;
			//buffer.WriteRemainingLength(_buffer.Length);

			return -1;
		}

		private static int Write(Memory<byte> buffer, UnsubAck packet)
		{
			return WriteShortPacket(buffer, PacketCodes.UnsubAck, packet.Id);
		}

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

		private static void AddWriter<T>(Func<Memory<byte>, T, int> writer) where T : Packet
		{
			writers.Add(typeof(T), (w, p) => writer(w, (T)p));
		}
	}
}