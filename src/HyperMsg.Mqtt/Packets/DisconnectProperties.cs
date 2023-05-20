namespace HyperMsg.Mqtt.Packets;

public class DisconnectProperties
{
    public uint SessionExpiryInterval { get; set; }

    public string ReasonString { get; set; }

    public Dictionary<string, string> UserProperties { get; set; }

    public string ServerReference { get; set; }
}
