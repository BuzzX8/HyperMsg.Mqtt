namespace HyperMsg.Mqtt;

public readonly record struct SubscriptionRequest(string TopicName, QosLevel Qos)
{
    public void Deconstruct(out string topic, out QosLevel qos)
    {
        topic = TopicName;
        qos = Qos;
    }
}
