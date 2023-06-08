using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Encoding
{
    const byte ConnectCode = 0b00010000;

    private static void Encode(Span<byte> buffer, Connect connect, out int bytesWritten)
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

        var offset = 0;
        //Fixed header
        buffer.WriteByte(ConnectCode, ref offset);
        buffer.WriteVarInt(contentLength, ref offset);
        //Variable header
        buffer.WriteString(connect.ProtocolName, ref offset);
        buffer.WriteByte(connect.ProtocolVersion, ref offset);
        buffer.WriteByte((byte)connect.Flags, ref offset);
        buffer.WriteUInt16(connect.KeepAlive, ref offset);
        //TODO: Properties shold be here
        buffer.WriteString(connect.ClientId, ref offset);

        if (connect.Flags.HasFlag(ConnectFlags.Will))
        {
            buffer.WriteString(connect.WillTopic, ref offset);
            buffer.WriteBinaryData(connect.WillPayload.Span, ref offset);
        }

        if (connect.Flags.HasFlag(ConnectFlags.UserName))
        {
            buffer.WriteString(connect.UserName, ref offset);
        }

        if (connect.Flags.HasFlag(ConnectFlags.Password))
        {
            buffer.WriteBinaryData(connect.Password.Span, ref offset);
        }

        bytesWritten = offset;
    }
}
