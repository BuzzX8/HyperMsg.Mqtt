using HyperMsg.Mqtt.Packets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Extensions
{
    public static class MessagingContextExtensions
    {
        public static MessagingTask<SessionState> ConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken = default) => 
            ConnectTask.StartNew(messagingContext, connectionSettings, cancellationToken);

        public static async Task DisconnectAsync(this IMessagingContext messagingContext, CancellationToken cancellationToken = default)
        {
            await messagingContext.Sender.SendAsync(Disconnect.Instance, cancellationToken);
            //await messagingContext.Sender.SendAsync(ConnectionCommand.Close, cancellationToken);
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

        public static Task<MessagingTask<bool>> PingAsync(this IMessagingContext messagingContext, CancellationToken cancellationToken = default)
        {
            return new PingTask(messagingContext, cancellationToken).StartAsync();
        }
    }
}
