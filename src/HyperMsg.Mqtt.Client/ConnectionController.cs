using System;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectionController : IConnectionController
    {
        private readonly ITransportCommandSender transportCommandSender;
        private readonly IMessageSender<Packet> messageSender;        
        private readonly MqttConnectionSettings connectionSettings;

        private TaskCompletionSource<SessionState> taskCompletionSource;

        public ConnectionController(ITransportCommandSender transportCommandSender, IMessageSender<Packet> messageSender, MqttConnectionSettings connectionSettings)
        {
            this.transportCommandSender = transportCommandSender ?? throw new ArgumentNullException(nameof(transportCommandSender));
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            this.connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }

        public async Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken cancellationToken = default)
        {
            await transportCommandSender.SendAsync(TransportCommand.Open, cancellationToken);

            if (connectionSettings.UseTls)
            {
                await transportCommandSender.SendAsync(TransportCommand.SetTransportLevelSecurity, cancellationToken);
            }

            var connectPacket = CreateConnectPacket(cleanSession);
            await messageSender.SendAsync(connectPacket, cancellationToken);

            taskCompletionSource = new TaskCompletionSource<SessionState>();

            return await taskCompletionSource.Task;
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await messageSender.SendAsync(Disconnect.Instance, cancellationToken);
            await transportCommandSender.SendAsync(TransportCommand.Close, cancellationToken);
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

        public Task HandleAsync(ConnAck connAck, CancellationToken cancellationToken)
        {
            if (taskCompletionSource == null)
            {
                return Task.CompletedTask;
            }

            taskCompletionSource.SetResult(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
            taskCompletionSource = null;
            return Task.CompletedTask;
        }
    }
}
