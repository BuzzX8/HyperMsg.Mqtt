using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectTask : MessagingTask<SessionState>
    {
        private readonly MqttConnectionSettings connectionSettings;

        internal ConnectTask(IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken) : base(messagingContext, cancellationToken)
        {
            this.connectionSettings = connectionSettings;
        }

        internal async Task<ConnectTask> RunAsync()
        {
            RegisterReceiveHandler<ConnAck>(OnConAckReceived);
            await Sender.TransmitConnectAsync(connectionSettings, CancellationToken);
            return this;
        }

        private void OnConAckReceived(ConnAck connAck)
        {
            var result = connAck.SessionPresent ? SessionState.Present : SessionState.Clean;
            SetResult(result);
        }
    }
}
