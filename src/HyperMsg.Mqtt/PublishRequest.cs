using System;

namespace HyperMsg.Mqtt
{
    public class PublishRequest
    {
        public PublishRequest(string topicName, ReadOnlyMemory<byte> message, QosLevel qos = QosLevel.Qos0)
        {
            TopicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
            Message = message;
            Qos = qos;
        }

        public string TopicName { get; }        

        public ReadOnlyMemory<byte> Message { get; }

        public QosLevel Qos { get; }

        public bool RetainMessage { get; }
    }
}
