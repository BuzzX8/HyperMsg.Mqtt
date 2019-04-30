using System;
using System.Collections.Generic;
using System.Text;

namespace HyperMsg.Mqtt.Client
{
    public class PublishRequest
    {
        public PublishRequest(string topicName, ReadOnlyMemory<byte> message)
        {
            TopicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
            Message = message;
        }

        public string TopicName { get; }        

        public ReadOnlyMemory<byte> Message { get; }

        public QosLevel Qos { get; }

        public bool RetainMessage { get; }
    }
}
