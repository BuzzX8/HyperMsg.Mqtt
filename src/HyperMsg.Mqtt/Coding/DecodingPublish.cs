using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    private static readonly Dictionary<byte, PropertyUpdater<PublishProperties>> PublishPropertyUpdaters = new()
    {
        [0x01] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) => p.PayloadFormatIndicator = b.ReadByte(ref offset),
        [0x02] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) => p.MessageExpiryInterval = b.ReadUInt32(ref offset),
        [0x03] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ContentType = b.ReadString(ref offset),
        [0x08] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ResponseTopic = b.ReadString(ref offset),
        [0x09] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) => p.CorrelationData = b.ReadBinaryData(ref offset),
        [0x0B] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) => p.SubscriptionIdentifier = b.ReadVarInt(ref offset),
        [0x23] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) => p.TopicAlias = b.ReadUInt16(ref offset),
        [0x26] = (PublishProperties p, ReadOnlySpan<byte> b, ref int offset) =>
        {
            p.UserProperties ??= new Dictionary<string, string>();
            ReadUserProperty(p.UserProperties, b, ref offset);
        }
    };

    private static Publish DecodePublish(ReadOnlySpan<byte> buffer)
    {
        var offset = 1;
        var flags = buffer[0] & 0x0F;
        var packetSize = buffer.ReadVarInt(ref offset);
        var topicName = buffer.ReadString(ref offset);
        var id = buffer.ReadUInt16(ref offset);

        var payload = buffer[offset..].ToArray();
        var qos = (QosLevel)((flags >> 1) & 0x03);
        var properties = DecodePublishProperies(buffer, ref offset);

        var packet = new Publish(id, topicName, payload, qos)
        {
            Retain = Convert.ToBoolean(flags & 0x01),
            Dup = Convert.ToBoolean(flags >> 3),
            Properties = properties
        };

        return packet;
    }

    private static PublishProperties DecodePublishProperies(ReadOnlySpan<byte> buffer, ref int offset) => DecodeProperties(buffer, PublishPropertyUpdaters, ref offset);
}
