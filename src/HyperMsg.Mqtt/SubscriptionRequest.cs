﻿using System;

namespace HyperMsg.Mqtt
{
    public class SubscriptionRequest
    {
        public SubscriptionRequest(string topicName, QosLevel qos)
        {
            TopicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
            Qos = qos;
        }

        public string TopicName { get; }

        public QosLevel Qos { get; }

        public void Deconstruct(out string topic, out QosLevel qos) 
        {
            topic = TopicName;
            qos = Qos;
        }
    }
}
