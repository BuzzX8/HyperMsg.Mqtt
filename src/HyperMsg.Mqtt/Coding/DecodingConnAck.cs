using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    private static readonly Dictionary<byte, PropertyUpdater<ConnAckProperties>> ConnAckPropertyUpdaters = new()
    {
        [0x11] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.SessionExpiryInterval = b.ReadUInt32(ref offset),
        [0x12] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.AssignedClientIdentifier = b.ReadString(ref offset),
        [0x13] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ServerKeepAlive = b.ReadUInt16(ref offset),
        [0x15] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.AuthenticationMethod = b.ReadString(ref offset),
        [0x16] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.AuthenticationData = b.ReadBinaryData(ref offset),
        [0x1A] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ResponseInformation = b.ReadString(ref offset),
        [0x1C] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ServerReference = b.ReadString(ref offset),
        [0x1F] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ReasonString = b.ReadString(ref offset),
        [0x21] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ReceiveMaximum = b.ReadUInt16(ref offset),
        [0x22] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.TopicAliasMaximum = b.ReadUInt16(ref offset),
        [0x24] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.MaximumQos = b.ReadByte(ref offset),
        [0x25] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.RetainAvailable = b.ReadBoolean(ref offset),
        [0x26] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) =>
        {
            p.UserProperties ??= new Dictionary<string, string>();
            ReadUserProperty(p.UserProperties, b, ref offset);
        },
        [0x27] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.MaximumPacketSize = b.ReadUInt32(ref offset),
        [0x28] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.WildcardSubscriptionAvailable = b.ReadBoolean(ref offset),
        [0x29] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.SubscriptionIdentifierAvailable = b.ReadBoolean(ref offset),
        [0x2A] = (ConnAckProperties p, ReadOnlySpan<byte> b, ref int offset) => p.SharedSubscriptionAvailable = b.ReadBoolean(ref offset),
    };

    private static ConnAck DecodeConnAck(ReadOnlySpan<byte> buffer)
    {
        var offset = 1;
        var length = buffer.ReadVarInt(ref offset);

        var flags = buffer.ReadByte(ref offset);
        var sessionPresent = Convert.ToBoolean(flags & 0x01);
        var reasonCode = (ConnectReasonCode)buffer.ReadByte(ref offset);
        var properties = DecodeConnAckProperties(buffer, ref offset);

        return new(reasonCode, sessionPresent, properties);
    }

    private static ConnAckProperties DecodeConnAckProperties(ReadOnlySpan<byte> buffer, ref int offset) => DecodeProperties(buffer, ConnAckPropertyUpdaters, ref offset);
}
