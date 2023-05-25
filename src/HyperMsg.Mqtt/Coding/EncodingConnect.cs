using HyperMsg.Mqtt.Packets;
using System.Buffers.Binary;

namespace HyperMsg.Mqtt.Coding;

public static partial class Encoding
{
    const byte ConnectCode = 0b00010000;

    public static void Encode(IBufferWriter writer, Connect connect)
    {
        var contentLength = 10 + GetStringByteCount(connect.ClientId);

        if (connect.Flags.HasFlag(ConnectFlags.Will))
        {
            contentLength += GetStringByteCount(connect.WillTopic);
            contentLength += connect.WillPayload.Length + 2;
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
        int written = buffer.Span[1..].WriteVarInt(contentLength);
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
            span = writer.GetSpan(connect.WillPayload.Length + sizeof(ushort));
            BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)connect.WillPayload.Length);
            connect.WillPayload.Span.CopyTo(span[2..]);
            writer.Advance(connect.WillPayload.Length + 2);
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
            //connect.Password.CopyTo(span[2..]);
            writer.Advance(connect.Password.Length + 2);
        }
    }
}
