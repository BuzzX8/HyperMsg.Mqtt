using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    internal class ConnectHandler
    {
        private readonly ISender<Packet> sender;
        private readonly MqttConnectionSettings connectionSettings;

        private TaskCompletionSource<SessionState> taskCompletionSource;

        internal ConnectHandler(ISender<Packet> sender, MqttConnectionSettings connectionSettings)
        {
            this.sender = sender;
            this.connectionSettings = connectionSettings;
        }

        internal async Task<SessionState> SendConnectAsync(bool cleanSession, CancellationToken token)
        {
            var connectPacket = CreateConnectPacket(cleanSession);
            await sender.SendAsync(connectPacket, token);

            taskCompletionSource = new TaskCompletionSource<SessionState>();

            return await taskCompletionSource.Task;
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

        internal void OnConnAckReceived(ConnAck connAck)
        {
            if (taskCompletionSource == null)
            {
                return;
            }

            taskCompletionSource.SetResult(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
            taskCompletionSource = null;
        }
    }
}
