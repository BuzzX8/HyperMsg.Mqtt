namespace HyperMsg.Mqtt.Packets;

public record Unsubscribe(ushort Id, IEnumerable<string> TopicFilters)
{
    public static implicit operator Packet(Unsubscribe packet) => packet;
}