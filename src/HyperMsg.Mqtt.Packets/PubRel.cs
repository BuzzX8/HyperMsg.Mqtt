namespace HyperMsg.Mqtt.Packets;

public record struct PubRel(ushort Id)
{
    public Packet ToPacket() => new(PacketType.PubRel, this);

    public static implicit operator Packet(PubRel pubRel) => pubRel.ToPacket();
}
