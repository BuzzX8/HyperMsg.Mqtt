namespace HyperMsg.Mqtt.Packets;

public record struct PubRel(ushort Id)
{
    public static implicit operator Packet(PubRel pubRel) => pubRel;
}
