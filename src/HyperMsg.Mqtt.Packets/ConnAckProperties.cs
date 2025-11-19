namespace HyperMsg.Mqtt.Packets;

public class ConnAckProperties
{
    public uint SessionExpiryInterval { get; internal set; }
    public ushort ReceiveMaximum { get; internal set; }
    public uint MaximumPacketSize { get; internal set; }
    public ushort TopicAliasMaximum { get; internal set; }
    public byte MaximumQos { get; internal set; }
    public bool RetainAvailable { get; internal set; }
    public string AssignedClientIdentifier { get; internal set; }
    public string ReasonString { get; internal set; }
    public Dictionary<string, string> UserProperties { get; internal set; }
    public bool WildcardSubscriptionAvailable { get; internal set; }
    public bool SubscriptionIdentifierAvailable { get; internal set; }
    public bool SharedSubscriptionAvailable { get; internal set; }
    public ushort ServerKeepAlive { get; internal set; }
    public string ResponseInformation { get; internal set; }
    public string ServerReference { get; internal set; }
    public string AuthenticationMethod { get; internal set; }
    public ReadOnlyMemory<byte> AuthenticationData { get; internal set; }
}
