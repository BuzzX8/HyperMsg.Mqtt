using System;
using System.Collections.Generic;
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

        public static Task<SubscriptionTask> StartSubscriptionAsync(this IMessagingContext messagingContext, IEnumerable<(string, QosLevel)> requests, CancellationToken cancellationToken)
        {
            return new SubscriptionTask(messagingContext, requests, cancellationToken).RunAsync();
        }

        public static Task<UnsubscriptionTask> StartUnsubscriptionAsync(this IMessagingContext messagingContext, IEnumerable<string> topics, CancellationToken cancellationToken)
        {
            return new UnsubscriptionTask(messagingContext, topics, cancellationToken).RunAsync();
        }

        public static Task<PublishTask> StartPublishAsync(this IMessagingContext messagingContext, PublishRequest request, CancellationToken cancellationToken)
        {
            return new PublishTask(messagingContext, request, cancellationToken).RunAsync();
        }
    }
}
