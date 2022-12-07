namespace HyperMsg.Mqtt.Packets;

public record ConnAck(ConnectionResult ResultCode, bool SessionPresent = false)
{
    public override string ToString() => $"ConnAck(SP={SessionPresent},Code={ResultCode})";
}
