using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    internal class ConnectHandler
    {
        private readonly ISender<Packet> sender;
        private readonly MqttConnectionSettings connectionSettings;

        private TaskCompletionSource<SessionState> tsc;

        internal ConnectHandler(ISender<Packet> sender, MqttConnectionSettings connectionSettings)
        {
            this.sender = sender;
            this.connectionSettings = connectionSettings;
        }

        internal async Task<SessionState> SendConnectAsync(bool cleanSession, CancellationToken token)
        {
            var connectPacket = CreateConnectPacket(cleanSession);
            await sender.SendAsync(connectPacket, token);

            tsc = new TaskCompletionSource<SessionState>();

            return await tsc.Task;
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

        internal void OnConnAckReceived(ConnAck connAck)
        {
            tsc.SetResult(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
        }
    }
}
