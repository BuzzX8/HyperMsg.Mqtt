namespace HyperMsg.Mqtt.Packets;

public class Subscribe
{
    public Subscribe(ushort id) => Id = id;

    public Subscribe(ushort id, IEnumerable<SubscriptionRequest> requests) : this(id)
    {
        Requests.AddRange(requests);
    }

    public Subscribe(ushort id, params SubscriptionRequest[] requests) : this(id)
    {
        Requests.AddRange(requests);
    }

    public ushort Id { get; }

    public List<SubscriptionRequest> Requests { get; } = new();

    public SubscribeProperties Properties { get; internal set; }

    public Packet ToPacket() => new(PacketType.Subscribe, this);

    public static implicit operator Packet(Subscribe subscribe) => subscribe.ToPacket();
}