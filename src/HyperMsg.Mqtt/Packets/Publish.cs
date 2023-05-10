namespace HyperMsg.Mqtt.Packets;

public class Publish : IEquatable<Publish>
{
    public Publish(ushort packetId, string topicName, ReadOnlyMemory<byte> payload, QosLevel qos)
    {
        ArgumentNullException.ThrowIfNull(topicName, nameof(topicName));

        Id = packetId;
        TopicName = topicName;
        Payload = payload;
        Qos = qos;
    }

    public ushort Id { get; }

    public bool Dup { get; set; }

    public QosLevel Qos { get; }

    public bool Retain { get; set; }

    public string TopicName { get; }

    public PublishProperties Properties { get; internal set; }

    public ReadOnlyMemory<byte> Payload { get; }

    public override int GetHashCode() => Id;

    public override bool Equals(object obj) => Equals(obj as Publish);

    public bool Equals(Publish packet)
    {
        return packet?.Dup == Dup
            && packet?.Qos == Qos
            && packet?.Retain == Retain
            && packet?.TopicName == TopicName
            && packet?.Id == Id;
    }

    internal Packet ToPacket() => new(PacketType.Publish, this);
}
