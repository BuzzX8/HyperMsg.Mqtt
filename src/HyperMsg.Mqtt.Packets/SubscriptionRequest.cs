namespace HyperMsg.Mqtt.Packets;

public class SubscriptionRequest
{
    public string TopicFilter { get; set; }

    public QosLevel MaxQos { get; set; }

    public bool NoLocal { get; set; }

    public bool RetainAsPublished { get; set; }

    public RetainHandlingOption RetainHandlingOption { get; set; }
}

public enum RetainHandlingOption : byte
{
    SendAtSubscribeTime = 0,
    SendIfSubscriptionDoesNotExists = 1,
    DonNotSend = 2,
}
