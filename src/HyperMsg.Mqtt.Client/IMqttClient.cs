using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public interface IMqttClient
    {
        Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default);

        Task DisconnectAsync(CancellationToken token = default);

        Task PublishAsync(PublishRequest request, CancellationToken token = default);

        Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken token = default);

        Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken token = default);

        Task PingAsync(CancellationToken token = default);

        event EventHandler<PublishReceivedEventArgs> PublishReceived;
    }
}