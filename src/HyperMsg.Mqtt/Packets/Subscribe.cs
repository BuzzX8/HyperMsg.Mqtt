namespace HyperMsg.Mqtt.Packets;

public record struct Subscribe(ushort Id, IEnumerable<SubscriptionRequest> Subscriptions);