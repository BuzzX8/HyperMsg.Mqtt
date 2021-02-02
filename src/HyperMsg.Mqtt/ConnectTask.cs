using HyperMsg.Mqtt.Packets;
using HyperMsg.Connection;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class ConnectTask : MessagingTask<SessionState>
    {
        private readonly MqttConnectionSettings connectionSettings;

        public ConnectTask(IMessagingContext context, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken) : base(context, cancellationToken)
        {
            this.connectionSettings = connectionSettings;
            RegisterReceiveHandler<ConnAck>(Handle);
        }

        internal async Task<MessagingTask<SessionState>> StartAsync()
        {
            await SendAsync(ConnectionCommand.Open, CancellationToken);

            if (connectionSettings.UseTls)
            {
                await SendAsync(ConnectionCommand.SetTransportLevelSecurity, CancellationToken);
            }

            var connectPacket = CreateConnectPacket(connectionSettings.CleanSession);
            await TransmitAsync(connectPacket, CancellationToken);

            return this;
        }

        private Connect CreateConnectPacket(bool cleanSession)
        {
            var flags = ConnectFlags.None;

            if (cleanSession)
            {
                flags |= ConnectFlags.CleanSession;
            }

            var connect = new Connect
            {
                ClientId = connectionSettings.ClientId,
                KeepAlive = connectionSettings.KeepAlive,
                Flags = flags
            };

            if (connectionSettings.WillMessageSettings != null)
            {
                connect.Flags |= ConnectFlags.Will;
                connect.WillTopic = connectionSettings.WillMessageSettings.Topic;
                connect.WillMessage = connectionSettings.WillMessageSettings.Message;
            }

            return connect;
        }

        private void Handle(ConnAck connAck) => Complete(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
    }
}
