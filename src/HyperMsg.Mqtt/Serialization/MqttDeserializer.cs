﻿using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Serialization
{
    public static class MqttDeserializer
    {
	    private static readonly Dictionary<byte, object> TwoBytePackets = new Dictionary<byte, object>
	    {
		    {PacketCodes.PingReq, new PingReq()},
		    {PacketCodes.PingResp, new PingResp()},
		    {PacketCodes.Disconnect, new Disconnect()}
	    };

	    private static readonly Dictionary<byte, Func<ReadOnlyMemory<byte>, int, object>> Readers = new Dictionary<byte, Func<ReadOnlyMemory<byte>, int, object>>
	    {
		    {PacketCodes.ConAck, ReadConAck },
		    {PacketCodes.Subscribe, ReadSubscribe},
		    {PacketCodes.Puback, ReadPuback},
		    {PacketCodes.Pubrec, ReadPubrec},
		    {PacketCodes.Pubrel, ReadPubrel},
		    {PacketCodes.Pubcomp, ReadPubcomp},
		    {PacketCodes.SubAck, ReadSubAck},
		    {PacketCodes.Unsubscribe, ReadUnsubscribe},
		    {PacketCodes.UnsubAck, ReadUnsubAck}
	    };

		internal static async Task<int> ReadBufferAsync(IMessageSender messageSender, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
			var (BytesConsumed, Packet) = Deserialize(buffer);

			if (BytesConsumed == 0)
            {
				return 0;
            }

			switch(Packet)
            {
				case ConnAck connAck:
					await messageSender.ReceiveAsync(connAck, cancellationToken);
					break;
            }

			return BytesConsumed;
        }

	    public static (int BytesConsumed, object Packet) Deserialize(ReadOnlySequence<byte> buffer)
	    {
            var span = buffer.First.Span;
            var code = span[0];
            buffer = buffer.Slice(1);
            (var length, var count) = buffer.First.ReadRemainingLength();
            var consumed = length + count + 1;

            if ((code & 0xf0) == 0x30)
            {
                var publish = ReadPublish(buffer.First.Slice(count), code, length);
                return (consumed, publish);
            }

            if (TwoBytePackets.ContainsKey(code))
            {
                var packet = GetTwoBytePacket(code, length);
                return (consumed, packet);
            }

            if (Readers.ContainsKey(code))
            {
                var packet = Readers[code](buffer.First.Slice(count), length);
                return (consumed, packet);
            }
            
            return (0, null);
        }

	    private static ConnAck ReadConAck(ReadOnlyMemory<byte> buffer, int length)
	    {
            var span = buffer.Span;
		    bool sessionPresent = span[0] == 1;
		    var code = (ConnectionResult)span[1];
		    return new ConnAck(code, sessionPresent);
	    }

	    private static Publish ReadPublish(ReadOnlyMemory<byte> buffer, byte code, int length)
	    {
		    bool dup = (code & 0x08) == 0x08;
		    QosLevel qos = (QosLevel)((code & 0x06) >> 1);
		    bool retain = (code & 0x01) == 1;
		    string topic = buffer.ReadString();
            buffer = buffer.Slice(Encoding.UTF8.GetByteCount(topic) + 2);
            var span = buffer.Span;
            ushort packetId = BinaryPrimitives.ReadUInt16BigEndian(span);
            buffer = buffer.Slice(2);
            span = buffer.Span;
		    int payloadLength = length - Encoding.UTF8.GetByteCount(topic) - 4;
            byte[] payload = span.Slice(0, payloadLength).ToArray();

		    return new Publish(packetId, topic, payload, qos)
		    {
			    Dup = dup,
			    Retain = retain
		    };
	    }

		private static object GetTwoBytePacket(byte code, int length)
	    {
		    if (length != 0)
		    {
			    throw new FormatException();
		    }

		    return TwoBytePackets[code];
	    }

	    private static object ReadSubscribe(ReadOnlyMemory<byte> buffer, int length)
	    {
		    return ReadPacketWithItems<(string, QosLevel)>(buffer, length, ReadTopicFilter, (id, items) => new Subscribe(id, items));
	    }

	    private static int ReadTopicFilter(ReadOnlyMemory<byte> buffer, Action<(string, QosLevel)> callback)
	    {
		    var topic = buffer.ReadString();
            var byteCount = Encoding.UTF8.GetByteCount(topic) + 2;
            buffer = buffer.Slice(byteCount);
            var span = buffer.Span;		    
		    callback((topic, (QosLevel)span[0]));
		    return Encoding.UTF8.GetByteCount(topic) + 3;
	    }

	    private static object ReadSubAck(ReadOnlyMemory<byte> buffer, int length)
	    {
		    return ReadPacketWithItems<SubscriptionResult>(buffer, length, ReadSubsResult, (id, res) => new SubAck(id, res));
	    }

	    private static int ReadSubsResult(ReadOnlyMemory<byte> buffer, Action<SubscriptionResult> callback)
	    {
            var span = buffer.Span;
		    callback((SubscriptionResult)span[0]);
		    return 1;
	    }

	    private static object ReadUnsubscribe(ReadOnlyMemory<byte> buffer, int length)
	    {
		    return ReadPacketWithItems<string>(buffer, length, ReadTopic, (id, items) => new Unsubscribe(id, items));
	    }

	    private static int ReadTopic(ReadOnlyMemory<byte> buffer, Action<string> callback)
	    {
		    string filter = buffer.ReadString();
		    return Encoding.UTF8.GetByteCount(filter) + 2;
	    }

	    private static object ReadPacketWithItems<T>(ReadOnlyMemory<byte> buffer, int length, Func<ReadOnlyMemory<byte>, Action<T>, int> readItem, Func<ushort, T[], object> createResult)
	    {
            var span = buffer.Span;
            ushort id = BinaryPrimitives.ReadUInt16BigEndian(span);
		    int totalRead = 2;
		    var items = new List<T>();
            buffer = buffer.Slice(totalRead);

		    while (totalRead < length)
		    {
			    int currentRead = readItem(buffer, items.Add);
                totalRead += currentRead;
                buffer = buffer.Slice(currentRead);
		    }

		    return createResult(id, items.ToArray());
	    }

		private static object ReadPuback(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubAck(id));

	    private static object ReadUnsubAck(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new UnsubAck(id));

	    private static object ReadPubcomp(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubComp(id));

	    private static object ReadPubrel(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubRel(id));

	    private static object ReadPubrec(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubRec(id));

	    private static object ReadPacketWithIdOnly(ReadOnlyMemory<byte> buffer, int length, Func<ushort, object> packetCreate)
	    {
		    if (length != 2)
		    {
			    throw new Exception();
		    }
            var span = buffer.Span;
            ushort id = BinaryPrimitives.ReadUInt16BigEndian(span);
		    return packetCreate(id);
	    }

		public static (int length, int byteCount) ReadRemainingLength(this ReadOnlyMemory<byte> buffer)
	    {
		    var span = buffer.Span;
		    int result = 0;
            int offset = 0;
            int i = 0;

            int value;
            do
            {
                value = span[i];
                result |= (value & 0x7f) << offset;
                offset += 7;
                i++;

                if (i == sizeof(int) && value >= 0x80)
                {
                    throw new FormatException();
                }
            }
            while ((value & 0x80) == 0x80);
            return (result, i);
	    }

	    public static string ReadString(this ReadOnlyMemory<byte> buffer)
	    {
		    ushort length = BinaryPrimitives.ReadUInt16BigEndian(buffer.Span);
		    var bytes = buffer.Slice(2, length).ToArray();
		    return Encoding.UTF8.GetString(bytes);
	    }
	}
}
