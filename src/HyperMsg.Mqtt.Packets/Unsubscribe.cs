namespace HyperMsg.Mqtt.Packets;

public record Unsubscribe(ushort Id, IEnumerable<string> TopicFilters)
{
    public Packet ToPacket() => new(PacketKind.Unsubscribe, this);

    public static implicit operator Packet(Unsubscribe packet) => packet.ToPacket();
}