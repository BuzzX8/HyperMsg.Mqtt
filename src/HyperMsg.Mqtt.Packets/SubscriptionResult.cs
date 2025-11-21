namespace HyperMsg.Mqtt.Packets;

public enum SubscriptionResult : byte
{
    SuccessQos0,
    SuccessQos1,
    SuccessQos2,
    Failure = 0x80
}