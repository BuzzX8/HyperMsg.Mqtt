using HyperMsg.Mqtt.Packets;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperMsg.Mqtt
{
    public static class MqttSerializer
    {
	    private static readonly byte[] Disconnect = { 0b11100000, 0b00000000 };
	    private static readonly byte[] PingResp = { 0b11010000, 0b00000000 };
	    private static readonly byte[] PingReq = { 0b11000000, 0b00000000 };
	    private static readonly byte[] ProtocolName = { 0, 4, (byte)'M', (byte)'Q', (byte)'T', (byte)'T', 4 };

	    const byte ConnectCode = 0b00010000;

        public static void Serialize(IBufferWriter<byte> writer, Connect connect)
	    {
            var contentLength = 10 + GetStringByteCount(connect.ClientId);

            if (connect.Flags.HasFlag(ConnectFlags.Will))
            {
                contentLength += GetStringByteCount(connect.WillTopic);
                contentLength += connect.WillMessage.Length + 2;
            }

            if (connect.Flags.HasFlag(ConnectFlags.UserName))
            {
                contentLength += GetStringByteCount(connect.UserName);
            }

            if (connect.Flags.HasFlag(ConnectFlags.Password))
            {
                contentLength += connect.Password.Length + 2;
            }

            var buffer = writer.GetMemory(contentLength);
            var span = buffer.Span;

            span[0] = ConnectCode;
            int written = buffer[1..].WriteRemainingLength(contentLength);
            writer.Advance(written + 1);

            writer.Write(ProtocolName);
            span = writer.GetSpan(3);
            span[0] = (byte)connect.Flags;
            BinaryPrimitives.WriteUInt16BigEndian(span[1..], connect.KeepAlive);
            writer.Advance(3);

            written = writer.WriteString(connect.ClientId);
            writer.Advance(written);

            if (connect.Flags.HasFlag(ConnectFlags.Will))
            {
                written = writer.WriteString(connect.WillTopic);
                writer.Advance(written);
                span = writer.GetSpan(connect.WillMessage.Length + sizeof(ushort));
                BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)connect.WillMessage.Length);
                connect.WillMessage.Span.CopyTo(span[2..]);
                writer.Advance(connect.WillMessage.Length + 2);
            }

            if (connect.Flags.HasFlag(ConnectFlags.UserName))
            {
                written = writer.WriteString(connect.UserName);
                writer.Advance(written);
            }

            if (connect.Flags.HasFlag(ConnectFlags.Password))
            {
                span = writer.GetSpan(connect.Password.Length + 2);
                BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)connect.Password.Length);
                connect.Password.CopyTo(span[2..]);
                writer.Advance(connect.Password.Length + 2);
            }
        }

        public static void Serialize(IBufferWriter<byte> writer, ConnAck connAck)
	    {
		    var span = writer.GetSpan(4);

		    span[0] = 0x20;
		    span[1] = 0x02;
		    span[2] = (byte)(connAck.SessionPresent ? 1 : 0);
		    span[3] = (byte)connAck.ResultCode;

			writer.Advance(4);
		}

	    public static void Serialize(IBufferWriter<byte> writer, Publish publish)
	    {
			byte header = 0b00110000;

		    if (publish.Dup)
		    {
			    header |= 0x08;
		    }

		    header |= (byte)((byte)publish.Qos << 1);

		    if (publish.Retain)
		    {
			    header |= 0x01;
		    }

			int contentLength = GetStringByteCount(publish.Topic) + sizeof(ushort) + publish.Message.Length;
		    var buffer = writer.GetMemory(5);//code + max length
		    var span = buffer.Span;

		    span[0] = header;
		    var written = buffer[1..].WriteRemainingLength(contentLength);
		    writer.Advance(written + 1);

		    written = writer.WriteString(publish.Topic);
			writer.Advance(written);
			
		    buffer = writer.GetMemory(publish.Message.Length + sizeof(ushort));
		    span = buffer.Span;
		    BinaryPrimitives.WriteUInt16BigEndian(span, publish.Id);
			publish.Message.CopyTo(buffer[sizeof(ushort)..]);
			writer.Advance(publish.Message.Length + sizeof(ushort));
	    }

	    public static void Serialize(IBufferWriter<byte> writer, PubAck pubAck) => WriteShortPacket(writer, PacketCodes.Puback, pubAck.Id);

	    public static void Serialize(IBufferWriter<byte> writer, PubRec pubRec) => WriteShortPacket(writer, PacketCodes.Pubrec, pubRec.Id);

	    public static void Serialize(IBufferWriter<byte> writer, PubRel pubRel) => WriteShortPacket(writer, PacketCodes.Pubrel, pubRel.Id);

		public static void Serialize(IBufferWriter<byte> writer, PubComp pubComp) => WriteShortPacket(writer, PacketCodes.Pubcomp, pubComp.Id);

	    public static void Serialize(IBufferWriter<byte> writer, Subscribe subscribe)
	    {
		    var contentLength = GetSubscriptionsByteCount(subscribe.Subscriptions) + sizeof(ushort);//ID + subscriptions
		    WriteHeaderWithLength(writer, PacketCodes.Subscribe, subscribe.Id, contentLength);

		    foreach (var (topic, qos) in subscribe.Subscriptions)
		    {
			    int written = writer.WriteString(topic);
				writer.Advance(written);
			    var span = writer.GetSpan(1);
			    span[0] = (byte)qos;
			    writer.Advance(1);
			}
		}

	    private static int GetSubscriptionsByteCount(IEnumerable<(string, QosLevel)> subscriptions) => subscriptions.Aggregate(0, (a, s) => a + GetStringByteCount(s.Item1) + 1);

		public static void Serialize(IBufferWriter<byte> writer, SubAck subAck)
		{
            var results = subAck.Results.ToArray();
			var contentLength = results.Length + sizeof(ushort);
			WriteHeaderWithLength(writer, PacketCodes.SubAck, subAck.Id, contentLength);

			var span = writer.GetSpan(results.Length);

			for (int i = 0; i < results.Length; i++)
			{
				span[i] = (byte) results[i];
			}

			writer.Advance(results.Length);
		}

	    public static void Serialize(IBufferWriter<byte> writer, Unsubscribe unsubscribe)
	    {
		    var contentLength = GetTopicsByteCount(unsubscribe.Topics) + sizeof(ushort);//ID + topics
		    WriteHeaderWithLength(writer, PacketCodes.Unsubscribe, unsubscribe.Id, contentLength);

			foreach (var topic in unsubscribe.Topics)
			{
				int written = writer.WriteString(topic);
				writer.Advance(written);
			}
		}

	    private static void WriteHeaderWithLength(IBufferWriter<byte> writer, byte code, ushort packetId, int contentLength)
	    {
		    var buffer = writer.GetMemory(5);//code + max length
		    var span = buffer.Span;

		    span[0] = code;
		    var written = buffer[1..].WriteRemainingLength(contentLength);
		    writer.Advance(written + 1);

		    span = writer.GetSpan(sizeof(ushort));
		    BinaryPrimitives.WriteUInt16BigEndian(span, packetId);
		    writer.Advance(sizeof(ushort));
		}

	    private static int GetTopicsByteCount(IEnumerable<string> topics) => topics.Aggregate(0, (a, s) => a + GetStringByteCount(s));

	    private static int GetStringByteCount(string str) => string.IsNullOrEmpty(str) ? 0 : Encoding.UTF8.GetByteCount(str) + sizeof(ushort);

		public static void Serialize(IBufferWriter<byte> writer, UnsubAck unsubAck) => WriteShortPacket(writer, PacketCodes.UnsubAck, unsubAck.Id);

	    private static void WriteShortPacket(IBufferWriter<byte> writer, byte code, ushort packetId)
	    {
		    var buffer = writer.GetMemory(4);
		    var span = buffer.Span;

		    span[0] = code;
		    span[1] = 2; //Remaining length always 2
		    BinaryPrimitives.WriteUInt16BigEndian(span[2..], packetId);
		    writer.Advance(4);
	    }

		public static void Serialize(IBufferWriter<byte> writer, PingReq _) => writer.Write(PingReq);

	    public static void Serialize(IBufferWriter<byte> writer, PingResp _) => writer.Write(PingResp);

		public static void Serialize(IBufferWriter<byte> writer, Disconnect _) => writer.Write(Disconnect);

	    public static int WriteRemainingLength(this Memory<byte> buffer, int length)
	    {
		    var span = buffer.Span;

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
		    var span = writer.GetSpan(Encoding.UTF8.GetByteCount(value) + sizeof(ushort));
		    var bytes = Encoding.UTF8.GetBytes(value);
			BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)bytes.Length);
		    bytes.CopyTo(span[sizeof(ushort)..]);
		    return bytes.Length + sizeof(ushort);
	    }
	}
}