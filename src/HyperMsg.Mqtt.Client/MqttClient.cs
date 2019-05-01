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

        public MqttClient(IConnection connection, ISender<Packet> sender, MqttConnectionSettings connectionSettings)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }

        public SessionState Connect(bool cleanSession = false) => ConnectAsync(cleanSession).GetAwaiter().GetResult();

        public async Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default)
        {
            await connection.OpenAsync(token);
            var connectPacket = CreateConnectPacket(cleanSession);
            await sender.SendAsync(connectPacket, token);

            return SessionState.Clean;
        }

        private Connect CreateConnectPacket(bool cleanSession)
        {
            var flags = ConnectFlags.None;

            if (cleanSession)
            {
                flags |= ConnectFlags.CleanSession;
            }

            return new Connect
            {
                ClientId = connectionSettings.ClientId,
                Flags = flags
            };
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
