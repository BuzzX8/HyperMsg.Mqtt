using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public interface IMqttClient
    {
        SessionState Connect(bool cleanSession = false);

        Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default);

        void Disconnect();

        Task DisconnectAsync(CancellationToken token = default);

        void Publish(PublishRequest request);

        Task PublishAsync(PublishRequest request, CancellationToken token = default);

        IEnumerable<QosLevel> Subscribe(IEnumerable<SubscriptionRequest> requests);

        Task<IEnumerable<QosLevel>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken token = default);

        void Unsubscribe(IEnumerable<string> topics);

        Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken token = default);

        void Ping();

        Task PingAsync(CancellationToken token);

        event EventHandler<PublishReceivedEventArgs> PublishReceived;
    }
}