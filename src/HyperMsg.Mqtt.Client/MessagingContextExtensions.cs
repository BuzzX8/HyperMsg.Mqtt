﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public static class MessagingContextExtensions
    {
        public static Task<ConnectTask> StartConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            return new ConnectTask(messagingContext, connectionSettings, cancellationToken).RunAsync();
        }

        public static async Task<SubscriptionTask> StartSubscriptionAsync(this IMessagingContext messagingContext, IEnumerable<(string, QosLevel)> requests, CancellationToken cancellationToken)
        {
            var task = new SubscriptionTask(messagingContext.Observable);
            await task.RunAsync(messagingContext.Sender, requests, cancellationToken);
            return task;
        }

        public static async Task<UnsubscriptionTask> StartUnsubscriptionAsync(this IMessagingContext messagingContext, IEnumerable<string> topics, CancellationToken cancellationToken)
        {
            var task = new UnsubscriptionTask(messagingContext.Observable);
            await task.RunAsync(messagingContext.Sender, topics, cancellationToken);
            return task;
        }
    }
}
