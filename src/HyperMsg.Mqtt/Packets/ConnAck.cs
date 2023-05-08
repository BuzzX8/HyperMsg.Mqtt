namespace HyperMsg.Mqtt.Packets;

public record ConnAck(ConnectReasonCode ReasonCode, bool SessionPresent = false, ConnAckProperties Properties = null)
{
    public override string ToString() => $"ConnAck(SP={SessionPresent},Code={ReasonCode})";
}
