using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Serialization
{
    public static class DeserializationExtensions
    {
	    private static readonly Dictionary<byte, Packet> TwoBytePackets = new Dictionary<byte, Packet>
	    {
		    {PacketCodes.PingReq, new PingReq()},
		    {PacketCodes.PingResp, new PingResp()},
		    {PacketCodes.Disconnect, new Disconnect()}
	    };

	    private static readonly Dictionary<byte, Func<ReadOnlyMemory<byte>, int, Packet>> Readers = new Dictionary<byte, Func<ReadOnlyMemory<byte>, int, Packet>>
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

		public static async Task<Packet> ReadMqttPacketAsync(CancellationToken token = default)
	    {
		    return ReadMqttPacket(new ReadOnlyMemory<byte>());
	    }

	    public static Packet ReadMqttPacket(ReadOnlyMemory<byte> buffer)
	    {
		    var span = buffer.Span;
		    var code = span[0];
            buffer = buffer.Slice(1);
		    (var length, var count) = buffer.ReadRemainingLength();
            
		    if ((code & 0xf0) == 0x30)
		    {
			    return ReadPublish(buffer.Slice(count), code, length);
		    }

			if (TwoBytePackets.ContainsKey(code))
		    {
			    return GetTwoBytePacket(code, length);
		    }

		    if (Readers.ContainsKey(code))
		    {
			    return Readers[code](buffer.Slice(count), length);
		    }

			throw new Exception();
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

		    return new Publish(packetId)
		    {
			    Dup = dup,
			    Qos = qos,
			    Retain = retain,
			    Topic = topic,
			    Message = payload
		    };
	    }

		private static Packet GetTwoBytePacket(byte code, int length)
	    {
		    if (length != 0)
		    {
			    throw new Exception();
		    }

		    return TwoBytePackets[code];
	    }

	    private static Packet ReadSubscribe(ReadOnlyMemory<byte> buffer, int length)
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

	    private static Packet ReadSubAck(ReadOnlyMemory<byte> buffer, int length)
	    {
		    return ReadPacketWithItems<SubscriptionResult>(buffer, length, ReadSubsResult, (id, res) => new SubAck(id, res));
	    }

	    private static int ReadSubsResult(ReadOnlyMemory<byte> buffer, Action<SubscriptionResult> callback)
	    {
            var span = buffer.Span;
		    callback((SubscriptionResult)span[0]);
		    return 1;
	    }

	    private static Packet ReadUnsubscribe(ReadOnlyMemory<byte> buffer, int length)
	    {
		    return ReadPacketWithItems<string>(buffer, length, ReadTopic, (id, items) => new Unsubscribe(id, items));
	    }

	    private static int ReadTopic(ReadOnlyMemory<byte> buffer, Action<string> callback)
	    {
		    string filter = buffer.ReadString();
		    return Encoding.UTF8.GetByteCount(filter) + 2;
	    }

	    private static Packet ReadPacketWithItems<T>(ReadOnlyMemory<byte> buffer, int length, Func<ReadOnlyMemory<byte>, Action<T>, int> readItem, Func<ushort, T[], Packet> createResult)
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

		private static Packet ReadPuback(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubAck(id));

	    private static Packet ReadUnsubAck(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new UnsubAck(id));

	    private static Packet ReadPubcomp(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubComp(id));

	    private static Packet ReadPubrel(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubRel(id));

	    private static Packet ReadPubrec(ReadOnlyMemory<byte> buffer, int length) => ReadPacketWithIdOnly(buffer, length, id => new PubRec(id));

	    private static Packet ReadPacketWithIdOnly(ReadOnlyMemory<byte> buffer, int length, Func<ushort, Packet> packetCreate)
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
		    int value = 0;
		    int offset = 0;
		    int i = 0;

		    do
		    {
			    value = span[i];
			    result |= (value & 0x7f) << offset;
			    offset += 7;
			    i++;

                if (i == sizeof(int) && value >= 0x80)
                {
                    throw new Exception();
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
