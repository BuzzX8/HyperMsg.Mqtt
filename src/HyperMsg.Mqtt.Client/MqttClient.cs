using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClient : IMqttClient
    {
        private readonly IConnection connection;
        private readonly ISender<Packet> sender;
        
        private readonly ConnectHandler connectHandler;
        private readonly SubscriptionHandler subscriptionHandler;

        public MqttClient(IConnection connection, ISender<Packet> sender, MqttConnectionSettings connectionSettings)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            connectHandler = new ConnectHandler(sender, connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings)));
            subscriptionHandler = new SubscriptionHandler(sender);
        }

        public SessionState Connect(bool cleanSession = false) => ConnectAsync(cleanSession).GetAwaiter().GetResult();

        public async Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default)
        {
            await connection.OpenAsync(token);
            return await connectHandler.SendConnectAsync(cleanSession, token);
        }

        public void Disconnect() => DisconnectAsync().GetAwaiter().GetResult();

        public async Task DisconnectAsync(CancellationToken token = default)
        {
            await sender.SendAsync(Mqtt.Disconnect.Instance, token);
            await connection.CloseAsync(token);
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

        public IEnumerable<SubscriptionResult> Subscribe(IEnumerable<SubscriptionRequest> requests) => SubscribeAsync(requests).GetAwaiter().GetResult();

        public Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken token = default)
        {
            _ = requests ?? throw new ArgumentNullException(nameof(requests));
            return subscriptionHandler.SendSubscribeAsync(requests, token);
        }

        public void Unsubscribe(IEnumerable<string> topics)
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void OnPacketReceived(Packet packet)
        {
            switch (packet)
            {
                case ConnAck connAck:
                    connectHandler.OnConnAckReceived(connAck);
                    break;

                case SubAck subAck:
                    subscriptionHandler.OnSubAckReceived(subAck);
                    break;
            }
        }

        public event EventHandler<PublishReceivedEventArgs> PublishReceived;
    }
}
