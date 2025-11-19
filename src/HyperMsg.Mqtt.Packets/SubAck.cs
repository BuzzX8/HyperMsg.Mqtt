namespace HyperMsg.Mqtt.Packets;

public record struct SubAck(ushort Id, IEnumerable<SubscriptionResult> Results);