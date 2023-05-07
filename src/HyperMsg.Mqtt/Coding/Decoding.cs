using HyperMsg.Mqtt.Packets;
using System;
using System.Buffers.Binary;

namespace HyperMsg.Mqtt.Coding
{
    public static class Decoding
    {
        public static (PacketType packetType, int packetSize) ReadFixedHeader(ReadOnlySpan<byte> buffer)
        {
            var packetType = (PacketType)(buffer[0] >> 4);
            var (packetSize, _) = ReadVarInt(buffer[1..]);

            return (packetType, packetSize);
        }

        public static object Decode(ReadOnlySpan<byte> buffer, out int bytesRead)
        {
            var (packetType, packetLength) = ReadFixedHeader(buffer);
            bytesRead = packetLength;

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

        public static Connect DecodeConnect(ReadOnlySpan<byte> buffer)
        {
            var offset = 1;
            var packetSize = buffer.ReadVarInt(ref offset);

            //Variable header
            var protocolName = buffer.ReadString(ref offset);
            var protocolVersion = buffer.ReadByte(ref offset);
            var flags = (ConnectFlags)buffer.ReadByte(ref offset);
            var keepAlive = buffer.ReadUInt16(ref offset);
            var properties = DecodeConnectProperties(buffer, protocolVersion, ref offset);

            //Payload
            var clientId = buffer.ReadString(ref offset);
            var connect = new Connect
            {
                ProtocolName = protocolName,
                ProtocolVersion = protocolVersion,
                Flags = flags,
                KeepAlive = keepAlive,
                ClientId = clientId,
                Properties = properties
            };

            if (flags.HasFlag(ConnectFlags.Will))
            {
                ReadWillFields(connect, buffer, ref offset);
            }

            if (flags.HasFlag(ConnectFlags.UserName))
            {
                connect.UserName = buffer.ReadString(ref offset);
            }

            if (flags.HasFlag(ConnectFlags.Password))
            {
                connect.Password = buffer.ReadBinaryData(ref offset);
            }

            return connect;
        }

        private static ConnectProperties DecodeConnectProperties(ReadOnlySpan<byte> buffer, byte protocolVersion, ref int offset)
        {
            if (protocolVersion < 5)
            {
                return default;
            }

            var propLength = buffer.ReadVarInt(ref offset);

            if (propLength == 0)
            {
                return default;
            }

            var properties = new ConnectProperties();
            var propBuffer = buffer[offset..(offset + propLength)];

            ReadConnectProperties(properties, propBuffer);

            offset += propLength;

            return properties;
        }

        private static void ReadConnectProperties(ConnectProperties properties, ReadOnlySpan<byte> propBuffer)
        {
            var offset = 0;

            while (offset < propBuffer.Length)
            {
                var propCode = propBuffer.ReadByte(ref offset);

                ReadConnectProperty(properties, propCode, propBuffer, ref offset);
            }
        }

        private static void ReadConnectProperty(ConnectProperties properties, byte propCode, ReadOnlySpan<byte> buffer, ref int offset)
        {
            switch (propCode)
            {
                case 0x11:
                    properties.SessionExpiryInterval = buffer.ReadUInt32(ref offset);
                    break;

                case 0x15:
                    properties.AuthenticationMethod = buffer.ReadString(ref offset);
                    break;

                case 0x16:
                    properties.AuthenticationData = Array.Empty<byte>();
                    break;

                case 0x17:
                    properties.RequestProblemInformation = Convert.ToBoolean(buffer.ReadByte(ref offset));
                    break;

                case 0x19:
                    properties.RequestResponseInformation = Convert.ToBoolean(buffer.ReadByte(ref offset));
                    break;

                case 0x21:
                    properties.ReceiveMaximum = buffer.ReadUInt16(ref offset);
                    break;

                case 0x22:
                    properties.TopicAliasMaximum = buffer.ReadUInt16(ref offset);
                    break;

                case 0x26:
                    var propName = buffer.ReadString(ref offset);
                    var propValue = buffer.ReadString(ref offset);
                    properties.UserProperties ??= new Dictionary<string, string>();
                    properties.UserProperties[propName] = propValue;
                    break;

                case 0x27:
                    properties.MaximumPacketSize = buffer.ReadUInt32(ref offset);
                    break;

                default:
                    throw new DecodingError("Invalid property code provided");
            }
        }

        private static void ReadWillFields(Connect connect, ReadOnlySpan<byte> buffer, ref int offset)
        {
            connect.WillProperties = DecodeWillProperties(buffer, connect.ProtocolVersion, ref offset);
            connect.WillTopic = buffer.ReadString(ref offset);
            connect.WillPayload = buffer.ReadBinaryData(ref offset);
        }

        private static ConnectWillProperties DecodeWillProperties(ReadOnlySpan<byte> buffer, byte protocolVersion, ref int offset)
        {
            if (protocolVersion < 5)
            {
                return default;
            }

            var willPropLength = buffer.ReadVarInt(ref offset);

            if (willPropLength == 0)
            {
                return null;
            }

            var props = new ConnectWillProperties();
            var propBuffer = buffer[offset..(offset + willPropLength)];

            ReadWillProperties(props, propBuffer);

            offset += willPropLength;

            return props;
        }

        private static void ReadWillProperties(ConnectWillProperties properties, ReadOnlySpan<byte> propBuffer)
        {
            var offset = 0;

            while (offset < propBuffer.Length)
            {
                var propCode = propBuffer.ReadByte(ref offset);

                ReadWillProperty(properties, propCode, propBuffer, ref offset);
            }
        }

        private static void ReadWillProperty(ConnectWillProperties properties, byte propCode, ReadOnlySpan<byte> propBuffer, ref int offset)
        {
            switch (propCode)
            {
                case 0x01:
                    properties.PayloadFormatIndicator = propBuffer.ReadByte(ref offset);
                    break;

                case 0x02:
                    properties.MessageExpiryInterval = propBuffer.ReadUInt32(ref offset);
                    break;

                case 0x03:
                    properties.ContentType = propBuffer.ReadString(ref offset);
                    break;

                case 0x08:
                    properties.ResponseTopic = propBuffer.ReadString(ref offset);
                    break;

                case 0x09:
                    properties.CorrelationData = propBuffer.ReadBinaryData(ref offset);
                    break;

                case 0x18:
                    properties.WillDelayInterval = propBuffer.ReadUInt32(ref offset);
                    break;

                default:
                    throw new DecodingError($"Incorrect property code for last will property ({propCode})");
            }
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
            string topic = "";// buffer.ReadString();
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
            //var topic = buffer.ReadString();
            //var byteCount = System.Text.Encoding.UTF8.GetByteCount(topic) + 2;
            //buffer = buffer[byteCount..];
            //var span = buffer.Span;
            //callback(new(topic, (QosLevel)span[0]));
            //return System.Text.Encoding.UTF8.GetByteCount(topic) + 3;
            return default;
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
            string filter = "";// buffer.ReadString();
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

        public static ReadOnlyMemory<byte> ReadBinaryData(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            var length = buffer.ReadUInt16(ref offset);
            var data = buffer[offset..(length + offset)];
            offset += length;

            return new ReadOnlyMemory<byte>(data.ToArray());
        }

        public static (int value, byte byteCount) ReadVarInt(this ReadOnlySpan<byte> buffer)
        {
            int result = 0;
            int offset = 0;
            byte i = 0;

            int value;
            do
            {
                value = buffer[i];
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

        private static int ReadVarInt(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            var (value, byteCount) = ReadVarInt(buffer[offset..]);

            offset += byteCount;
            return value;
        }

        public static string ReadString(this ReadOnlySpan<byte> buffer)
        {
            var offset = 0;
            return ReadString(buffer, ref offset);
        }

        private static string ReadString(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            ushort length = ReadUInt16(buffer, ref offset);
            EnsureBufferSize(buffer, 2 + length);

            var str = System.Text.Encoding.UTF8.GetString(buffer[offset..(offset + length)]);
            offset += length;

            return str;
        }

        private static byte ReadByte(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            var value = buffer[offset];
            offset += 1;

            return value;
        }

        private static ushort ReadUInt16(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            EnsureBufferSize(buffer, sizeof(ushort));

            var value = BinaryPrimitives.ReadUInt16BigEndian(buffer[offset..]);
            offset += sizeof(ushort);

            return value;
        }

        private static uint ReadUInt32(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            EnsureBufferSize(buffer, sizeof(uint));

            var value = BinaryPrimitives.ReadUInt32BigEndian(buffer[offset..]);
            offset += sizeof(uint);

            return value;
        }

        private static void EnsureBufferSize(ReadOnlySpan<byte> buffer, int requiredSize)
        {
            if (buffer.Length < requiredSize)
            {
                throw new EncodingError("Buffer size less than required");
            }
        }
    }
}
