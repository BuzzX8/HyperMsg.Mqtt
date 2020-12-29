using HyperMsg.Mqtt.Packets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MessagingContextExtensions
    {
        public static Task<MessagingTask<SessionState>> ConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken = default)
        {
            return new ConnectTask(messagingContext, connectionSettings, cancellationToken).StartAsync();
        }

        public static Task<MessagingTask<IEnumerable<SubscriptionResult>>> SubscribeAsync(this IMessagingContext messagingContext, IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default)
        {
            return new SubscribeTask(messagingContext, cancellationToken).StartAsync(requests);
        }

        public static Task<MessagingTask<bool>> UnsubscribeAsync(this IMessagingContext messagingContext, IEnumerable<string> topics, CancellationToken token = default)
        {
            return new UnsubscribeTask(messagingContext, token).StartAsync(topics);
        }

        public static Task<MessagingTask<bool>> PublishAsync(this IMessagingContext messagingContext, PublishRequest request, CancellationToken cancellationToken = default)
        {
            return new PublishTask(messagingContext, cancellationToken).StartAsync(request);
        }
    }
}
