namespace HyperMsg.Mqtt.Packets;

public class ConnectWillProperties
{
    public byte PayloadFormatIndicator { get; set; }

    public uint MessageExpiryInterval { get; set; }

    public uint WillDelayInterval { get; set; }

    public string? ContentType { get; set; }

    public string? ResponseTopic { get; set; }

    public ReadOnlyMemory<byte> CorrelationData { get; set; }

    public IDictionary<string, string>? UserProperties { get; set; }
}
