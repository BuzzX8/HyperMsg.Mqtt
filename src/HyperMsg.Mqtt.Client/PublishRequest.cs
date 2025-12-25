namespace HyperMsg.Mqtt;

public class PublishRequest(string topicName, ReadOnlyMemory<byte> message, QosLevel qos = QosLevel.Qos0, bool retainMessage = false)
{
    public string TopicName { get; } = topicName ?? throw new ArgumentNullException(nameof(topicName));

    public ReadOnlyMemory<byte> Message { get; } = message;

    public QosLevel Qos { get; } = qos;

    public bool RetainMessage { get; } = retainMessage;
}
