using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
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

    private static PublishProperties DecodePublishProperies(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var propLength = buffer.ReadVarInt(ref offset);

        if (propLength == 0)
        {
            return default;
        }

        var properties = new PublishProperties();
        var propBuffer = buffer[offset..(offset + propLength)];

        ReadPublishProperties(properties, propBuffer);

        return properties;
    }

    private static void ReadPublishProperties(PublishProperties properties, ReadOnlySpan<byte> propBuffer)
    {
        var offset = 0;

        while (offset < propBuffer.Length)
        {
            var propCode = propBuffer.ReadByte(ref offset);

            ReadPublishProperty(properties, propCode, propBuffer, ref offset);
        }
    }

    private static void ReadPublishProperty(PublishProperties properties, byte propCode, ReadOnlySpan<byte> buffer, ref int offset)
    {
        switch (propCode)
        {
            case 0x01:
                properties.PayloadFormatIndicator = buffer.ReadByte(ref offset);
                break;

            case 0x02:
                properties.MessageExpiryInterval = buffer.ReadUInt32(ref offset);
                break;

            case 0x03:
                properties.ContentType = buffer.ReadString(ref offset);
                break;

            case 0x08:
                properties.ResponseTopic = buffer.ReadString(ref offset);
                break;

            case 0x09:
                properties.CorrelationData = buffer.ReadBinaryData(ref offset);
                break;

            case 0x0B:
                properties.SubscriptionIdentifier = buffer.ReadVarInt(ref offset);
                break;

            case 0x23:
                properties.TopicAlias = buffer.ReadUInt16(ref offset);
                break;

            case 0x26:
                properties.UserProperties ??= new Dictionary<string, string>();
                ReadUserProperty(properties.UserProperties, buffer, ref offset);
                break;

            default:
                throw new DecodingError($"Incorrect Publish property code provided ({propCode})");
        }
    }
}
