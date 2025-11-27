using HyperMsg.Mqtt.Packets;
using System.Buffers;
using System.Buffers.Binary;

namespace HyperMsg.Mqtt.Coding;

public static partial class Encoding
{
    private static readonly byte[] Disconnect = { 0b11100000, 0b00000000 };
    private static readonly byte[] PingResp = { 0b11010000, 0b00000000 };
    private static readonly byte[] PingReq = { 0b11000000, 0b00000000 };

    public static ulong Encode(Memory<byte> buffer, Packet packet)
    {
        Encode(buffer.Span, packet, out var bytesEncoded);

        return (ulong)bytesEncoded;
    }

    public static void Encode(Span<byte> buffer, Packet packet, out int bytesWritten)
    {
        switch (packet.Kind)
        {
            case PacketKind.Connect:
                EncodeConnect(buffer, packet.ToConnect(), out bytesWritten);
                break;

            default:
                throw new NotSupportedException();
        }
    }

    public static void Encode(Span<byte> buffer, ConnAck connAck)
    {
        buffer[0] = 0x20;
        buffer[1] = 0x02;
        buffer[2] = (byte)(connAck.SessionPresent ? 1 : 0);
        buffer[3] = (byte)connAck.ReasonCode;
    }

    public static void Encode(Span<byte> buffer, Publish publish)
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

        //int contentLength = GetStringByteCount(publish.TopicName) + sizeof(ushort) + publish.Payload.Length;
        //var buffer = writer.GetMemory(5);//code + max length
        //var span = buffer.Span;

        //span[0] = header;
        //var written = buffer.Span[1..].WriteVarInt(contentLength);
        //writer.Advance(written + 1);

        //written = 0;// writer.WriteString(publish.TopicName);
        //writer.Advance(written);

        //buffer = writer.GetMemory(publish.Payload.Length + sizeof(ushort));
        //span = buffer.Span;
        //BinaryPrimitives.WriteUInt16BigEndian(span, publish.Id);
        //publish.Payload.CopyTo(buffer[sizeof(ushort)..]);
        //writer.Advance(publish.Payload.Length + sizeof(ushort));
    }

    public static void Encode(Span<byte> buffer, PubAck pubAck) => WriteShortPacket(buffer, PacketCodes.Puback, pubAck.Id);

    public static void Encode(Span<byte> buffer, PubRec pubRec) => WriteShortPacket(buffer, PacketCodes.Pubrec, pubRec.Id);

    public static void Encode(Span<byte> buffer, PubRel pubRel) => WriteShortPacket(buffer, PacketCodes.Pubrel, pubRel.Id);

    public static void Encode(Span<byte> buffer, PubComp pubComp) => WriteShortPacket(buffer, PacketCodes.Pubcomp, pubComp.Id);

    public static void Encode(Span<byte> buffer, Subscribe subscribe)
    {
        //var contentLength = GetSubscriptionsByteCount(subscribe.Subscriptions) + sizeof(ushort);//ID + subscriptions
        //WriteHeaderWithLength(writer, PacketCodes.Subscribe, subscribe.Id, contentLength);

        //foreach (var (topic, qos) in subscribe.Subscriptions)
        //{
        //    int written = writer.WriteString(topic);
        //    writer.Advance(written);
        //    var span = writer.GetSpan(1);
        //    span[0] = (byte)qos;
        //    writer.Advance(1);
        //}
    }

    //private static int GetSubscriptionsByteCount(IEnumerable<SubscriptionRequest> subscriptions) => subscriptions.Aggregate(0, (a, s) => a + GetStringByteCount(s.TopicName) + 1);

    public static void Encode(Span<byte> buffer, SubAck subAck)
    {
        var results = subAck.Results.ToArray();
        var contentLength = results.Length + sizeof(ushort);
        WriteHeaderWithLength(buffer, PacketCodes.SubAck, subAck.Id, contentLength);
        
        for (int i = 0; i < results.Length; i++)
        {
            buffer[i] = (byte)results[i];
        }
    }

    public static void Encode(Span<byte> buffer, Unsubscribe unsubscribe)
    {
        var contentLength = GetTopicsByteCount(unsubscribe.TopicFilters) + sizeof(ushort);//ID + topics
        WriteHeaderWithLength(buffer, PacketCodes.Unsubscribe, unsubscribe.Id, contentLength);

        //foreach (var topic in unsubscribe.TopicFilters)
        //{
        //    int written = 0;// writer.WriteString(topic);
        //    writer.Advance(written);
        //}
    }

    private static void WriteHeaderWithLength(Span<byte> buffer, byte code, ushort packetId, int contentLength)
    {
        buffer[0] = code;
        var written = buffer[1..].WriteVarInt(contentLength);
        //writer.Advance(written + 1);

        //span = writer.GetSpan(sizeof(ushort));
        //BinaryPrimitives.WriteUInt16BigEndian(span, packetId);
        //writer.Advance(sizeof(ushort));
    }

    private static int GetTopicsByteCount(IEnumerable<string> topics) => topics.Aggregate(0, (a, s) => a + GetStringByteCount(s));

    private static int GetStringByteCount(string str) => string.IsNullOrEmpty(str) ? 0 : System.Text.Encoding.UTF8.GetByteCount(str) + sizeof(ushort);

    public static void Encode(Span<byte> buffer, UnsubAck unsubAck) => WriteShortPacket(buffer, PacketCodes.UnsubAck, unsubAck.Id);

    private static void WriteShortPacket(Span<byte> buffer, byte code, ushort packetId)
    {
        buffer[0] = code;
        buffer[1] = 2; //Remaining length always 2
        BinaryPrimitives.WriteUInt16BigEndian(buffer[2..], packetId);
    }

    //public static void Encode(Span<byte> buffer, PingReq _) => writer.Write(PingReq);

    //public static void Encode(Span<byte> buffer, PingResp _) => writer.Write(PingResp);

    //public static void Encode(Span<byte> buffer, Disconnect _) => writer.Write(Disconnect);

    private static void WriteByte(this Span<byte> buffer, byte value, ref int offset)
    {
        buffer[offset] = value;
        offset += sizeof(byte);
    }

    private static void WriteUInt16(this Span<byte> buffer, ushort value, ref int offset)
    {
        BinaryPrimitives.WriteUInt16BigEndian(buffer[offset..], value);
        offset += sizeof(ushort);
    }

    private static void WriteUInt32(this Span<byte> buffer, uint value, ref int offset)
    {
        BinaryPrimitives.WriteUInt32BigEndian(buffer[offset..], value);
        offset += sizeof(uint);
    }

    private static void WriteVarInt(this Span<byte> buffer, int value, ref int offset) => offset += WriteVarInt(buffer[offset..], value);

    public static int WriteVarInt(this Span<byte> buffer, int value)
    {
        if (value > 0x1fffff)
        {
            buffer[0] = (byte)(value | 0x80);
            buffer[1] = (byte)(value >> 7 | 0x80);
            buffer[2] = (byte)(value >> 14 | 0x80);
            buffer[3] = (byte)(value >> 21);
            return 4;
        }

        if (value > 0x3fff)
        {
            buffer[0] = (byte)(value | 0x80);
            buffer[1] = (byte)(value >> 7 | 0x80);
            buffer[2] = (byte)(value >> 14);
            return 3;
        }

        if (value > 0x7f)
        {
            buffer[0] = (byte)(value | 0x80);
            buffer[1] = (byte)(value >> 7);
            return 2;
        }

        buffer[0] = (byte)value;
        return 1;
    }

    private static void WriteBinaryData(this Span<byte> buffer, ReadOnlySpan<byte> data, ref int offset)
    {
        buffer.WriteUInt16((ushort)data.Length, ref offset);
        data.CopyTo(buffer[offset..]);
    }

    public static void WriteString(this Span<byte> buffer, string value, ref int offset)
    {
        var length = System.Text.Encoding.UTF8.GetByteCount(value);

        if (length > ushort.MaxValue - sizeof(ushort))
        {
            throw new EncodingError($"String length too big. It must be less of equal {ushort.MaxValue - sizeof(ushort)}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(value);

        buffer.WriteUInt16((ushort)length, ref offset);
        bytes.CopyTo(buffer[offset..]);
        offset += bytes.Length;
    }
}