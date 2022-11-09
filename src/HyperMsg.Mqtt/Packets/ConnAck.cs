namespace HyperMsg.Mqtt.Packets;

public record class ConnAck(ConnectionResult ResultCode, bool SessionPresent = false)
{
    public override string ToString() => $"ConnAck(SP={SessionPresent},Code={ResultCode})";
}
