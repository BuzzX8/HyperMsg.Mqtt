namespace HyperMsg.Mqtt.Packets;

public record class Disconnect(DisconnectReasonCode ReasonCode = DisconnectReasonCode.NormalDisconnection)
{
}
