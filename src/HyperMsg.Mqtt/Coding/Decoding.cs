using HyperMsg.Mqtt.Packets;
using System.Buffers.Binary;

namespace HyperMsg.Mqtt.Coding
{
    public static class Decoding
    {
        public static (PacketType packetType, int packetSize) ReadFixedHeader(ReadOnlyMemory<byte> buffer)
        {
            var packetType = (PacketType)(buffer.Span[0] >> 4);
            var (packetSize, _) = ReadVarInt(buffer[1..]);

            return (packetType, packetSize);
        }

        public static object Decode(ReadOnlyMemory<byte> buffer, out int bytesConsumed)
        {
            var (packetType, packetLength) = ReadFixedHeader(buffer);
            bytesConsumed = packetLength;

            switch (packetType)
            {
                case PacketType.Connect:
                    return DecodeConnect(buffer);
                case PacketType.ConAck:
                    break;
                case PacketType.Publish:
                    break;
                case PacketType.PubAck:
                    break;
                case PacketType.PubRec:
                    break;
                case PacketType.PubRel:
                    break;
                case PacketType.PubComp:
                    break;
                case PacketType.Subscribe:
                    break;
                case PacketType.SubAck:
                    break;
                case PacketType.Unsubscribe:
                    break;
                case PacketType.UnsubAck:
                    break;
                case PacketType.PingReq:
                    break;
                case PacketType.PingResp:
                    break;
                case PacketType.Disconnect:
                    break;
                case PacketType.Auth:
                    break;
            }

            throw new DecodingError("Invalid packet type");
        }

        public static Connect DecodeConnect(ReadOnlyMemory<byte> buffer)
        {
            var (packetSize, byteCount) = buffer[1..].ReadVarInt();
            var offset = 1 + byteCount;
            var protocolName = buffer[offset..].ReadString();
            offset += System.Text.Encoding.UTF8.GetByteCount(protocolName) + 2;
            var protocolVersion = buffer.Span[offset];

            offset++;
            var connectFlags = (ConnectFlags)buffer.Span[offset];
            offset++;
            var keepAlive = BinaryPrimitives.ReadUInt16BigEndian(buffer.Span[offset..]);
            offset += 2;

            if (protocolVersion == 5)
            {
                //TODO: Read properties
            }

            var clientId = buffer[offset..].ReadString();

            return new Connect
            {
                ClientId = clientId,
                Flags = connectFlags,
                KeepAlive = keepAlive
            };
        }

        private static ConnAck ReadConAck(ReadOnlyMemory<byte> buffer, int length)
        {
            var span = buffer.Span;
            bool sessionPresent = span[0] == 1;
            var code = (ConnectionResult)span[1];
            return new ConnAck(code, sessionPresent);
        }

        private static Publish ReadPublish(ReadOnlyMemory<byte> buffer, byte code, uint length)
        {
            bool dup = (code & 0x08) == 0x08;
            QosLevel qos = (QosLevel)((code & 0x06) >> 1);
            bool retain = (code & 0x01) == 1;
            string topic = buffer.ReadString();
            buffer = buffer[(System.Text.Encoding.UTF8.GetByteCount(topic) + 2)..];
            var span = buffer.Span;
            ushort packetId = BinaryPrimitives.ReadUInt16BigEndian(span);
            buffer = buffer[2..];
            span = buffer.Span;
            //uint payloadLength = length - System.Text.Encoding.UTF8.GetByteCount(topic) - 4;
            //byte[] payload = span.Slice(0, payloadLength).ToArray();

            //return new Publish(packetId, topic, payload, qos)
            //{
            //    Dup = dup,
            //    Retain = retain
            //};
            return default;
        }

        private static object ReadSubscribe(ReadOnlyMemory<byte> buffer, int length)
        {
            return ReadPacketWithItems<SubscriptionRequest>(buffer, length, ReadTopicFilter, (id, items) => new Subscribe(id, items));
        }

        private static int ReadTopicFilter(ReadOnlyMemory<byte> buffer, Action<SubscriptionRequest> callback)
        {
            var topic = buffer.ReadString();
            var byteCount = System.Text.Encoding.UTF8.GetByteCount(topic) + 2;
            buffer = buffer[byteCount..];
            var span = buffer.Span;
            callback(new(topic, (QosLevel)span[0]));
            return System.Text.Encoding.UTF8.GetByteCount(topic) + 3;
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
            return System.Text.Encoding.UTF8.GetByteCount(filter) + 2;
        }

        private static object ReadPacketWithItems<T>(ReadOnlyMemory<byte> buffer, int length, Func<ReadOnlyMemory<byte>, Action<T>, int> readItem, Func<ushort, T[], object> createResult)
        {
            var span = buffer.Span;
            ushort id = BinaryPrimitives.ReadUInt16BigEndian(span);
            int totalRead = 2;
            var items = new List<T>();
            buffer = buffer[totalRead..];

            while (totalRead < length)
            {
                int currentRead = readItem(buffer, items.Add);
                totalRead += currentRead;
                buffer = buffer[currentRead..];
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

        public static (int value, byte byteCount) ReadVarInt(this ReadOnlyMemory<byte> buffer)
        {
            var span = buffer.Span;
            int result = 0;
            int offset = 0;
            byte i = 0;

            int value;
            do
            {
                value = span[i];
                result |= (value & 0x7f) << offset;
                offset += 7;
                i++;

                if (i == sizeof(int) && value >= 0x80)
                {
                    throw new DecodingError("VarInt incorrectly encoded");
                }
            }
            while ((value & 0x80) == 0x80);
            return (result, i);
        }

        public static string ReadString(this ReadOnlyMemory<byte> buffer)
        {
            ushort length = ReadUInt16(buffer);
            EnsureBufferSize(buffer, 2 + length);

            return System.Text.Encoding.UTF8.GetString(buffer.Slice(2, length).Span);
        }

        public static ushort ReadUInt16(this ReadOnlyMemory<byte> buffer)
        {
            EnsureBufferSize(buffer, sizeof(ushort));

            return BinaryPrimitives.ReadUInt16BigEndian(buffer.Span);
        }

        public static uint ReadUInt32(this ReadOnlyMemory<byte> buffer)
        {
            EnsureBufferSize(buffer, sizeof(uint));

            return BinaryPrimitives.ReadUInt32BigEndian(buffer.Span);
        }

        private static void EnsureBufferSize(ReadOnlyMemory<byte> buffer, int requiredSize)
        {
            if (buffer.Length < requiredSize)
            {
                throw new EncodingError("Buffer size less than required");
            }
        }
    }
}
