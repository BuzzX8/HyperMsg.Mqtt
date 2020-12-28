using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public interface IMqttClient
    {
        Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken cancellationToken = default);

        Task DisconnectAsync(CancellationToken cancellationToken = default);

        Task PublishAsync(PublishRequest request, CancellationToken cancellationToken = default);

        Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default);

        Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken cancellationToken = default);

        Task PingAsync(CancellationToken cancellationToken = default);

        event EventHandler<PublishReceivedEventArgs> PublishReceived;
    }
}