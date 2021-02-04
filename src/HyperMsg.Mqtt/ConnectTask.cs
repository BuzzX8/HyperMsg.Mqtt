using HyperMsg.Mqtt.Packets;
using HyperMsg.Connection;
using System.Threading;
using System.Threading.Tasks;
using HyperMsg.Mqtt.Extensions;

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

            await Sender.TransmitConnectionRequestAsync(connectionSettings, CancellationToken);

            return this;
        }

        private void Handle(ConnAck connAck) => Complete(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
    }
}
