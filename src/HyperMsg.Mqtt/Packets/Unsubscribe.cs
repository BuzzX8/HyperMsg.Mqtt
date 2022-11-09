namespace HyperMsg.Mqtt.Packets;

public record struct Unsubscribe(ushort Id, IEnumerable<string> Topics);