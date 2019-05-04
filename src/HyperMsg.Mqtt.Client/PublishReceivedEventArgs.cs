using System;

namespace HyperMsg.Mqtt.Client
{
    public class PublishReceivedEventArgs : EventArgs
    {
        public PublishReceivedEventArgs(string topicName, ReadOnlyMemory<byte> message)
        {
            TopicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
            Message = message;
        }

        public string TopicName { get; }

        public ReadOnlyMemory<byte> Message { get; }
    }
}
