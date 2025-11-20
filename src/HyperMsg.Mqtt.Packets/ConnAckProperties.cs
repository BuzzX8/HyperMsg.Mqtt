namespace HyperMsg.Mqtt.Packets;

public class ConnAckProperties
{
    public uint SessionExpiryInterval { get; set; }
    public ushort ReceiveMaximum { get; set; }
    public uint MaximumPacketSize { get; set; }
    public ushort TopicAliasMaximum { get; set; }
    public byte MaximumQos { get; set; }
    public bool RetainAvailable { get; set; }
    public string AssignedClientIdentifier { get; set; }
    public string ReasonString { get; set; }
    public Dictionary<string, string> UserProperties { get; set; }
    public bool WildcardSubscriptionAvailable { get; set; }
    public bool SubscriptionIdentifierAvailable { get; set; }
    public bool SharedSubscriptionAvailable { get; set; }
    public ushort ServerKeepAlive { get; set; }
    public string ResponseInformation { get; set; }
    public string ServerReference { get; set; }
    public string AuthenticationMethod { get; set; }
    public ReadOnlyMemory<byte> AuthenticationData { get; set; }
}
