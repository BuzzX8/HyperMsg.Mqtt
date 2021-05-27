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

        public static IMessagingTask PublishAsync(this IMessagingContext messagingContext, PublishRequest request) => 
            PublishTask.StartNew(messagingContext, request);

        public static IMessagingTask PingAsync(this IMessagingContext messagingContext) => PingTask.StartNew(messagingContext);
    }
}
