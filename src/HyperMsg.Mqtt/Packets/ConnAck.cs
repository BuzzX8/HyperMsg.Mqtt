namespace HyperMsg.Mqtt.Packets;

public record ConnAck(ConnectReasonCode ReasonCode, bool SessionPresent = false, ConnAckProperties Properties = null)
{
    public Packet ToPacket() => new(PacketType.ConAck, this);

    public override string ToString() => $"ConnAck(SP={SessionPresent},Code={ReasonCode})";

    public static implicit operator Packet(ConnAck connAck) => connAck.ToPacket();
}
