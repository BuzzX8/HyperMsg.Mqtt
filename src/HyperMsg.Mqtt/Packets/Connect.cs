namespace HyperMsg.Mqtt.Packets;

public record Connect
{
    internal Connect() { }

    #region Variable header

    public string ProtocolName { get; internal set; }

    public byte ProtocolVersion { get; internal set; }

    public ConnectFlags Flags { get; set; }

    public ushort KeepAlive { get; set; }

    public ConnectProperties Properties { get; set; }

    #endregion

    #region Payload

    public string ClientId { get; set; }

    public ConnectWillProperties WillProperties { get; set; }

    public string WillTopic { get; set; }

    public ReadOnlyMemory<byte> WillPayload { get; set; }

    public string UserName { get; set; }

    public ReadOnlyMemory<byte> Password { get; set; }

    #endregion

    internal Packet ToPacket() => new(PacketType.Connect, this);

    public override string ToString() => $"Connect(ClientId={ClientId})";

    public static Connect NewV5(string clientId)
    {
        return new()
        {
            ProtocolName = "MQTT",
            ProtocolVersion = 5,
            ClientId = clientId,
        };
    }

    public static implicit operator Packet(Connect connect) => connect.ToPacket();
}
