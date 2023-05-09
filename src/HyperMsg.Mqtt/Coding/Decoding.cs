using HyperMsg.Mqtt.Packets;
using System.Buffers.Binary;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    public static object Decode(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        var (packetType, packetLength) = ReadFixedHeader(buffer);

        EnsureBufferSize(buffer, packetLength + 1);

        bytesRead = packetLength;

        switch (packetType)
        {
            case PacketType.Connect:
                return DecodeConnect(buffer);
            case PacketType.ConAck:
                return DecodeConnAck(buffer);
            case PacketType.Publish:
                return DecodePublish(buffer);
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

    private static (PacketType packetType, int packetSize) ReadFixedHeader(ReadOnlySpan<byte> buffer)
    {
        var packetType = (PacketType)(buffer[0] >> 4);
        var (packetSize, _) = ReadVarInt(buffer[1..]);

        return (packetType, packetSize);
    }

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
