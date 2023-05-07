namespace HyperMsg.Mqtt.Packets;

public class Connect : IEquatable<Connect>
{
    public string ProtocolName { get; set; }

    public byte ProtocolVersion { get; set; }

    public ConnectFlags Flags { get; set; }

    public ushort KeepAlive { get; set; }

    public string ClientId { get; set; }

    public string WillTopic { get; set; }

    public ReadOnlyMemory<byte> WillMessage { get; set; }

    public string UserName { get; set; }

    public byte[] Password { get; set; }

    public ConnectProperties Properties { get; set; }

    public override int GetHashCode() => Flags.GetHashCode() ^ ClientId.GetHashCode();

    public override bool Equals(object obj) => Equals(obj as Connect);

    public bool Equals(Connect packet)
    {
        return packet?.Flags == Flags
               && packet.KeepAlive == KeepAlive
               && packet.ClientId == ClientId
               && packet.WillTopic == WillTopic
               && packet.UserName == UserName;
    }

    public override string ToString() => $"Connect(ClientId={ClientId})";
}
