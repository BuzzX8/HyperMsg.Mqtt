using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    private static readonly Dictionary<byte, PropertyUpdater<SubscribeProperties>> SubscribePropertyUpdaters = new()
    {
        [0x0B] = (SubscribeProperties p, ReadOnlySpan<byte> b, ref int offset) => p.SubscriptionIdentifier = b.ReadVarInt(ref offset),
        [0x26] = (SubscribeProperties p, ReadOnlySpan<byte> b, ref int offset) =>
        {
            p.UserProperties ??= new();
            ReadUserProperty(p.UserProperties, b, ref offset);
        }
    };

    private static Subscribe DecodeSubscribe(ReadOnlySpan<byte> buffer)
    {
        var offset = 1;
        var packetSize = buffer.ReadVarInt(ref offset);
        var id = buffer.ReadUInt16(ref offset);

        var requests = new List<SubscriptionRequest>();

        while (offset < packetSize)
        {
            var topicFilter = buffer.ReadString(ref offset);
            var options = buffer.ReadByte(ref offset);
            requests.Add(new()
            {
                MaxQos = (QosLevel)(options & 0x3),
                NoLocal = Convert.ToBoolean((options >> 2) & 0x01),
                RetainAsPublished = Convert.ToBoolean((options >> 3) & 0x01),
                RetainHandlingOption = (RetainHandlingOption)((options >> 4) & 0x03),
                TopicFilter = topicFilter,
            });
        }

        return new(id, requests.ToArray())
        {
            Properties = DecodeProperties(buffer, SubscribePropertyUpdaters, ref offset),
        };
    }
}
