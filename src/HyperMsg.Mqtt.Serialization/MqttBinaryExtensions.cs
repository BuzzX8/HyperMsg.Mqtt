using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MqttBinaryExtensions
	{
        private static readonly byte[] Disconnect = { 0b11100000, 0b00000000 };
		private static readonly byte[] PingResp = { 0b11010000, 0b00000000 };
		private static readonly byte[] PingReq = { 0b11000000, 0b00000000 };
        private static readonly byte[] ProtocolName = { (byte)'M', (byte)'Q', (byte)'T', (byte)'T' };

		const byte ConnectCode = 0b00010000;


		private static readonly Dictionary<Type, Func<PipeWriter, Packet, CancellationToken, ValueTask<FlushResult>>> writers;

		static MqttBinaryExtensions()
		{
			writers = new Dictionary<Type, Func<PipeWriter, Packet, CancellationToken, ValueTask<FlushResult>>>();
   //         AddWriter<Connect>(Write);
   //         AddWriter<ConnAck>(Write);
   //         AddWriter<Publish>(Write);
			AddWriter<PubAck>(Write);
			AddWriter<PubRec>(Write);
			AddWriter<PubRel>(Write);
			AddWriter<PubComp>(Write);
			//AddWriter<Subscribe>(Write);
			//AddWriter<SubAck>(Write);
			AddWriter<Unsubscribe>(Write);
			AddWriter<UnsubAck>(Write);
			AddWriter<PingReq>(Write);
			AddWriter<PingResp>(Write);
			AddWriter<Disconnect>(Write);
		}

		public static ValueTask<Packet> ReadPacket(this PipeReader reader, CancellationToken token = default)
		{
			throw new NotImplementedException();
		}

		public static int ReadRemainingLength(this ReadOnlyMemory<byte> buffer)
		{
            /*
             * int result = 0;
			int value = 0;
			int offset = 0;
			int i = 0;

			do
			{
				i++;
				value = reader.ReadByte();

				if (i == sizeof(int) && value >= 0x80)
				{
					throw new DeserializationException();
				}

				result |= (value & 0x7f) << offset;
				offset += 7;
			}
			while((value & 0x80) == 0x80);
			return result;*/
			throw new NotImplementedException();
		}

		public static string ReadString(this ReadOnlyMemory<byte> buffer)
		{
            /*
             * ushort length = reader.ReadUInt16();
		    var bytes = reader.ReadBytes(length);
		    return Encoding.UTF8.GetString(bytes, 0, bytes.Length);*/
			throw new NotImplementedException();
		}

        public static ValueTask<FlushResult> WritePacket(this PipeWriter writer, Packet packet, CancellationToken token = default) => writers[packet.GetType()](writer, packet, token);

        private static int Write(Memory<byte> buffer, Connect packet)
        {
            var span = buffer.Span;

            span[0] = ConnectCode;
            int offset = 1;
            int contentLength = 104;
            offset += WriteProtocolName(buffer, offset);

            //offset += buffer.WriteRemainingLength(contentLength, offset);
            span[offset] = 4;//Protocol level
            span[offset + 1] = (byte)packet.Flags;
            offset += 2;
            buffer.WriteUInt16BigEndian(packet.KeepAlive, offset);
            //_bufferWriter.WriteString(packet.ClientId);

            //if (packet.Flags.HasFlag(ConnectFlags.Will))
            //{
            //    _bufferWriter.WriteString(packet.WillTopic);
            //    _bufferWriter.Write((ushort)packet.WillMessage.Length);
            //    _bufferWriter.Write(packet.WillMessage, 0, packet.WillMessage.Length);
            //}

            //if (packet.Flags.HasFlag(ConnectFlags.UserName))
            //{
            //    _bufferWriter.WriteString(packet.UserName);
            //}

            //if (packet.Flags.HasFlag(ConnectFlags.Password))
            //{
            //    _bufferWriter.Write((ushort)packet.Password.Length);
            //    _bufferWriter.Write(packet.Password, 0, packet.Password.Length);
            //}

            return offset;
        }

        private static int WriteProtocolName(Memory<byte> buffer, int offset)
        {
            var span = buffer.Span;
            buffer.WriteUInt16BigEndian(0x0004, offset);
            ProtocolName.CopyTo(buffer.Slice(sizeof(ushort) + offset));

            return sizeof(ushort) + 4;
        }

        private static int Write(Memory<byte> buffer, ConnAck packet)
        {
            var span = buffer.Span;

            span[0] = 0x20;
            span[1] = 0x02;
            span[2] = (byte)(packet.SessionPresent ? 1 : 0);
            span[3] = (byte)packet.ResultCode;

            return 4;
        }

        private static int Write(Memory<byte> buffer, Publish packet)
        {
            byte header = 0b00110000;
            var span = buffer.Span;

            if (packet.Dup)
            {
                header |= 0x08;
            }

            header |= (byte)((byte)packet.Qos << 1);

            if (packet.Retain)
            {
                header |= 0x01;
            }

            span[0] = header;
            int offset = 1;
            int contentLength = GetStringByteCount(packet.Topic) + sizeof(ushort) + packet.Message.Length;
            //offset += buffer.WriteRemainingLength(contentLength, offset);

            //offset += buffer.WriteString(packet.Topic, offset);
            buffer.WriteUInt16BigEndian(packet.Id, offset);
            offset += sizeof(ushort);
            packet.Message.CopyTo(span.Slice(offset));
            
            return offset + packet.Message.Length;
        }

		private static ValueTask<FlushResult> Write(PipeWriter writer, PubAck packet, CancellationToken token) => WriteShortPacket(writer, PacketCodes.Puback, packet.Id, token);

		private static ValueTask<FlushResult> Write(PipeWriter writer, PubRec packet, CancellationToken token) => WriteShortPacket(writer, PacketCodes.Pubrec, packet.Id, token);

		private static ValueTask<FlushResult> Write(PipeWriter writer, PubRel packet, CancellationToken token) => WriteShortPacket(writer, PacketCodes.Pubrel, packet.Id, token);

		private static ValueTask<FlushResult> Write(PipeWriter writer, PubComp packet, CancellationToken token) => WriteShortPacket(writer, PacketCodes.Pubcomp, packet.Id, token);
                       
		private static ValueTask<FlushResult> Write(PipeWriter writer, Subscribe packet)
		{
            var buffer = writer.GetMemory();
			var span = buffer.Span;

			span[0] = PacketCodes.Subscribe;

			int contentLength = GetSubscriptionsByteCount(packet.Subscriptions) + sizeof(ushort);
            int offset = 0;// buffer.WriteRemainingLength(contentLength, 1) + 1;
			buffer.WriteUInt16BigEndian(packet.Id, offset);
			offset += sizeof(ushort);

			foreach (var (topic, qos) in packet.Subscriptions)
			{
				//offset += buffer.WriteString(topic, offset);
				span[offset] = (byte)qos;
				offset++;
			}

			return new ValueTask<FlushResult>();
		}

		private static int GetSubscriptionsByteCount((string, QosLevel)[] subscriptions) => subscriptions.Aggregate(0, (a, s) => a + GetStringByteCount(s.Item1) + 1);

		private static int Write(Memory<byte> buffer, SubAck packet)
		{
			var span = buffer.Span;

			span[0] = PacketCodes.SubAck;

			int contentLength = packet.Results.Length + sizeof(ushort);
            int offset = 0;// buffer.WriteRemainingLength(contentLength, 1) + 1;
			buffer.WriteUInt16BigEndian(packet.Id, offset);
			offset += sizeof(ushort);

			foreach (var result in packet.Results)
			{
				span[offset] = (byte)result;
				offset++;
			}

			return offset;
		}

		private static ValueTask<FlushResult> Write(PipeWriter writer, Unsubscribe packet, CancellationToken token)
		{
            int count = 1;
            var buffer = writer.GetMemory(1);
            buffer.Span[0] = PacketCodes.Unsubscribe;
            writer.Advance(1);
            int contentLength = GetTopicsByteCount(packet.Topics) + sizeof(ushort);
            writer.WriteRemainingLength(contentLength);

            foreach (var topic in packet.Topics)
            {
                writer.WriteString(topic);
            }

            writer.Advance(count);

			return writer.FlushAsync(token);
		}

		private static int GetTopicsByteCount(string[] topics) => topics.Aggregate(0, (a, s) => a + GetStringByteCount(s));

		private static int GetStringByteCount(string str) => Encoding.UTF8.GetByteCount(str) + sizeof(ushort);

		private static ValueTask<FlushResult> Write(PipeWriter writer, UnsubAck packet, CancellationToken token) => WriteShortPacket(writer, PacketCodes.UnsubAck, packet.Id, token);

		private static ValueTask<FlushResult> WriteShortPacket(PipeWriter writer, byte code, ushort packetId, CancellationToken token)
		{
            var buffer = writer.GetMemory(4);
            var span = buffer.Span;

			span[0] = code;
			span[1] = 2; //Remaining length always 2
			buffer.WriteUInt16BigEndian(packetId, 2);
            writer.Advance(4);
			return writer.FlushAsync(token);
		}

        private static ValueTask<FlushResult> Write(PipeWriter writer, PingReq packet, CancellationToken token) => writer.WriteAsync(PingReq);

        private static ValueTask<FlushResult> Write(PipeWriter writer, PingResp packet, CancellationToken token) => writer.WriteAsync(PingResp, token);

        private static ValueTask<FlushResult> Write(PipeWriter writer, Disconnect packet, CancellationToken token) => writer.WriteAsync(Disconnect, token);

        public static int WriteRemainingLength(this PipeWriter writer, int length)
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

			span[0] = (byte) length;
			return 1;
		}
        
		public static int WriteString(this PipeWriter writer, string value)
		{
            //var bytes = Encoding.UTF8.GetBytes(value);
            //buffer.WriteUInt16BigEndian((ushort)bytes.Length, offset);
            //bytes.CopyTo(buffer.Slice(offset + sizeof(ushort)));
            //return bytes.Length + sizeof(ushort);
            throw new NotImplementedException();
		}

		private static void AddWriter<T>(Func<PipeWriter, T, CancellationToken, ValueTask<FlushResult>> writer) where T : Packet => writers.Add(typeof(T), (w, p, c) => writer(w, (T)p, c));
	}
}