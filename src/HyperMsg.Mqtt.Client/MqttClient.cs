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
        private readonly MqttConnectionSettings connectionSettings;
        
        private readonly ConnectHandler connectHandler;

        public MqttClient(IConnection connection, ISender<Packet> sender, MqttConnectionSettings connectionSettings)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
            connectHandler = new ConnectHandler(sender, connectionSettings);
        }

        public SessionState Connect(bool cleanSession = false) => ConnectAsync(cleanSession).GetAwaiter().GetResult();

        public async Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default)
        {
            await connection.OpenAsync(token);
            return await connectHandler.SendConnectAsync(cleanSession, token);
        }

        public void Disconnect() => DisconnectAsync().GetAwaiter().GetResult();

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

        public void OnPacketReceived(Packet packet)
        {
            switch (packet)
            {
                case ConnAck connAck:
                    connectHandler.OnConnAckReceived(connAck);
                    break;
            }
        }

        public event EventHandler<PublishReceivedEventArgs> PublishReceived;
    }
}
