using HyperMsg.Mqtt.Packets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MessagingContextExtensions
    {
        public static IMessagingTask<SessionState> ConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken = default) => 
            ConnectTask.StartNew(messagingContext, connectionSettings, cancellationToken);

        public static async Task DisconnectAsync(this IMessagingContext messagingContext, CancellationToken cancellationToken = default)
        {
            await messagingContext.Sender.SendAsync(Disconnect.Instance, cancellationToken);
        }

        public static SubscribeTask SubscribeAsync(this IMessagingContext messagingContext, IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default) => 
            SubscribeTask.StartNew(requests, messagingContext, cancellationToken);

        public static Task<MessagingTask<bool>> UnsubscribeAsync(this IMessagingContext messagingContext, IEnumerable<string> topics, CancellationToken token = default)
        {
            return new UnsubscribeTask(messagingContext, token).StartAsync(topics);
        }

        public static Task<MessagingTask<bool>> PublishAsync(this IMessagingContext messagingContext, PublishRequest request, CancellationToken cancellationToken = default)
        {
            return new PublishTask(messagingContext, cancellationToken).StartAsync(request);
        }

        public static PingTask PingAsync(this IMessagingContext messagingContext, CancellationToken cancellationToken = default) => PingTask.StartNew(messagingContext, cancellationToken);
    }
}
