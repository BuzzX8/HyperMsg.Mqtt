using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClient : IMqttClient
    {
        public SessionState Connect(bool cleanSession = false)
        {
            throw new NotImplementedException();
        }

        public Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Ping()
        {
            throw new NotImplementedException();
        }

        public Task PingAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Publish(PublishRequest request)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync(PublishRequest request, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QosLevel> Subscribe(IEnumerable<SubscriptionRequest> requests)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<QosLevel>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IEnumerable<string> topics)
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<PublishReceivedEventArgs> PublishReceived;
    }
}
