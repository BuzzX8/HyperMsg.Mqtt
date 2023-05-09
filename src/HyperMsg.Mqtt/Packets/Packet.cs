namespace HyperMsg.Mqtt.Packets;

public readonly struct Packet
{
    private readonly object packet;

    internal Packet(PacketType type, object packet)
    {
        this.packet = packet;
        Type = type;
    }

    public PacketType Type { get; }

    public bool IsConnect => Type == PacketType.Connect;

    public bool IsConnAck => Type == PacketType.ConAck;

    public bool IsPublish => Type == PacketType.Publish;

    public Connect ToConnect() => To<Connect>();

    public ConnAck ToConnAck() => To<ConnAck>();

    public Publish ToPublish() => To<Publish>();

    private T To<T>() => (T)packet;
}
