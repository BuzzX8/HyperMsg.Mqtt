namespace HyperMsg.Mqtt.Packets;

/// <summary>
/// Represents a generic MQTT packet, encapsulating the packet type and its associated data.
/// </summary>
public readonly struct Packet
{
    private readonly object packet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Packet"/> struct with the specified type and packet data.
    /// </summary>
    /// <param name="type">The type of the MQTT packet.</param>
    /// <param name="packet">The packet data object.</param>
    private Packet(PacketKind type, object packet)
    {
        this.packet = packet;
        Kind = type;
    }

    /// <summary>
    /// Gets the type of the MQTT packet.
    /// </summary>
    public PacketKind Kind { get; }

    /// <summary>
    /// Gets a value indicating whether this packet is a CONNECT packet.
    /// </summary>
    public bool IsConnect => Kind == PacketKind.Connect;

    /// <summary>
    /// Gets a value indicating whether this packet is a CONNACK packet.
    /// </summary>
    public bool IsConnAck => Kind == PacketKind.ConAck;

    /// <summary>
    /// Gets a value indicating whether this packet is a PUBLISH packet.
    /// </summary>
    public bool IsPublish => Kind == PacketKind.Publish;

    /// <summary>
    /// Gets a value indicating whether this packet is a SUBSCRIBE packet.
    /// </summary>
    public bool IsSubscribe => Kind == PacketKind.Subscribe;

    /// <summary>
    /// Converts this packet to a <see cref="Connect"/> instance.
    /// </summary>
    /// <returns>The <see cref="Connect"/> representation of this packet.</returns>
    public Connect ToConnect() => To<Connect>();

    /// <summary>
    /// Converts this packet to a <see cref="ConnAck"/> instance.
    /// </summary>
    /// <returns>The <see cref="ConnAck"/> representation of this packet.</returns>
    public ConnAck ToConnAck() => To<ConnAck>();

    /// <summary>
    /// Converts this packet to a <see cref="Publish"/> instance.
    /// </summary>
    /// <returns>The <see cref="Publish"/> representation of this packet.</returns>
    public Publish ToPublish() => To<Publish>();

    /// <summary>
    /// Converts this packet to a <see cref="Subscribe"/> instance.
    /// </summary>
    /// <returns>The <see cref="Subscribe"/> representation of this packet.</returns>
    public Subscribe ToSubscribe() => To<Subscribe>();

    private T To<T>() => (T)packet;

    public static Packet From(Connect connect) => new(PacketKind.Connect, connect);

    public static Packet From(ConnAck connAck) => new(PacketKind.ConAck, connAck);

    public static Packet From(Publish publish) => new(PacketKind.Publish, publish);

    public static Packet From(Subscribe subscribe) => new(PacketKind.Subscribe, subscribe);
}
