using HyperMsg.Mqtt.Packets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MessagingContextExtensions
    {
        public static IMessagingTask<SessionState> ConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings) => 
            ConnectTask.StartNew(messagingContext, connectionSettings);

        public static async Task DisconnectAsync(this IMessagingContext messagingContext, CancellationToken cancellationToken = default) => 
            await messagingContext.Sender.SendAsync(Disconnect.Instance, cancellationToken);

        public static IMessagingTask<IEnumerable<SubscriptionResult>> SubscribeAsync(this IMessagingContext messagingContext, IEnumerable<SubscriptionRequest> requests) => 
            SubscribeTask.StartNew(requests, messagingContext);

        public static IMessagingTask UnsubscribeAsync(this IMessagingContext messagingContext, IEnumerable<string> topics) => 
            UnsubscribeTask.StartNew(topics, messagingContext);

        public static Task<MessagingTask<bool>> PublishAsync(this IMessagingContext messagingContext, PublishRequest request, CancellationToken cancellationToken = default)
        {
            return new PublishTask(messagingContext, cancellationToken).StartAsync(request);
        }

        public static PingTask PingAsync(this IMessagingContext messagingContext, CancellationToken cancellationToken = default) => PingTask.StartNew(messagingContext, cancellationToken);
    }
}
