using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MessagingContextExtensions
    {
        public static async Task<IDisposable> ConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, Action<ConnAck> responseCallback, CancellationToken cancellationToken = default)
        {
            var subscription = messagingContext.HandlersRegistry.RegisterReceivePipeHandler(responseCallback);
            await messagingContext.Sender.SendConnectionRequestAsync(connectionSettings, cancellationToken);
            return subscription;
        }

        public static async Task DisconnectAsync(this IMessagingContext messagingContext, CancellationToken cancellationToken = default) => 
            await messagingContext.Sender.SendAsync(Disconnect.Instance, cancellationToken);

        public static Task<IDisposable> SubscribeAsync(this IMessagingContext messagingContext, IEnumerable<SubscriptionRequest> requests) =>
            throw new NotImplementedException();

        public static Task<IDisposable> UnsubscribeAsync(this IMessagingContext messagingContext, IEnumerable<string> topics) =>
            throw new NotImplementedException();

        public static Task<IDisposable> PublishAsync(this IMessagingContext messagingContext, PublishRequest request) =>
            throw new NotImplementedException();

        public static Task<IDisposable> PingAsync(this IMessagingContext messagingContext) => throw new NotImplementedException();
    }
}
