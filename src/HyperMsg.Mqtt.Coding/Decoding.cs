using HyperMsg.Coding;
using HyperMsg.Mqtt.Packets;
using System.Buffers.Binary;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    public static DecodingResult<Packet> Decode(ReadOnlyMemory<byte> buffer)
    {
        var packet = Decode(buffer.Span, out var bytesRead);

        return new DecodingResult<Packet>(packet, (ulong)bytesRead);
    }

    public static Packet Decode(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        var (packetType, packetLength) = ReadFixedHeader(buffer);

        EnsureBufferSize(buffer, packetLength + 1);

        bytesRead = packetLength;

        switch (packetType)
        {
            case PacketType.Connect:
                return DecodeConnect(buffer).ToPacket();
            case PacketType.ConAck:
                return DecodeConnAck(buffer).ToPacket();
            case PacketType.Publish:
                return DecodePublish(buffer).ToPacket();
            case PacketType.PubAck:
                break;
            case PacketType.PubRec:
                break;
            case PacketType.PubRel:
                break;
            case PacketType.PubComp:
                break;
            case PacketType.Subscribe:
                return DecodeSubscribe(buffer).ToPacket();
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

    private static (PacketType packetType, int packetSize) ReadFixedHeader(ReadOnlySpan<byte> buffer)
    {
        var packetType = (PacketType)(buffer[0] >> 4);
        var (packetSize, _) = ReadVarInt(buffer[1..]);

        return (packetType, packetSize);
    }

    private static T DecodeProperties<T>(ReadOnlySpan<byte> buffer, IDictionary<byte, PropertyUpdater<T>> updaters, ref int offset) where T : class, new()
    {
        var propLength = buffer.ReadVarInt(ref offset);

        if (propLength == 0)
        {
            return default;
        }

        var properties = new T();
        var propBuffer = buffer[offset..(offset + propLength)];

        ReadProperties(properties, propBuffer, updaters);

        offset += propLength;

        return properties;
    }

    private static void ReadProperties<T>(T properties, ReadOnlySpan<byte> propBuffer, IDictionary<byte, PropertyUpdater<T>> updaters)
    {
        var offset = 0;

        while (offset < propBuffer.Length)
        {
            var propCode = propBuffer.ReadByte(ref offset);

            ReadProperty(properties, propCode, propBuffer, updaters, ref offset);
        }
    }

    private static void ReadProperty<T>(T properties, byte propCode, ReadOnlySpan<byte> buffer, IDictionary<byte, PropertyUpdater<T>> updaters, ref int offset)
    {
        if (!updaters.ContainsKey(propCode))
        {
            throw new DecodingError($"Incorrect property code provided ({propCode})");
        }

        updaters[propCode].Invoke(properties, buffer, ref offset);
    }

    internal delegate void PropertyUpdater<T>(T properties, ReadOnlySpan<byte> buffer, ref int offset);

    private static void ReadUserProperty(IDictionary<string, string> properties, ReadOnlySpan<byte> buffer, ref int offset)
    {
        var propName = buffer.ReadString(ref offset);
        var propValue = buffer.ReadString(ref offset);

        properties[propName] = propValue;
    }

    private static bool ReadBoolean(this ReadOnlySpan<byte> buffer, ref int offset)
    {
        var value = ReadByte(buffer, ref offset);

        if (value > 1)
        {
            throw new DecodingError("Incorrect boolean value");
        }

        return Convert.ToBoolean(value);
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

    private static int ReadVarInt(this ReadOnlySpan<byte> buffer, ref int offset)
    {
        var (value, byteCount) = ReadVarInt(buffer[offset..]);

        offset += byteCount;
        return value;
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

    public static ReadOnlyMemory<byte> ReadBinaryData(this ReadOnlySpan<byte> buffer, ref int offset)
    {
        var length = buffer.ReadUInt16(ref offset);
        var data = buffer[offset..(length + offset)];
        offset += length;

        return new ReadOnlyMemory<byte>(data.ToArray());
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

    private static void EnsureBufferSize(ReadOnlySpan<byte> buffer, int requiredSize)
    {
        if (buffer.Length < requiredSize)
        {
            throw new EncodingError("Buffer size less than required");
        }
    }
}
