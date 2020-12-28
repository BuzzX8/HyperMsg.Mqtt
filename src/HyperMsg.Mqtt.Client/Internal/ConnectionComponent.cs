using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectionComponent
    {        
        private readonly IMessageSender messageSender;        
        private readonly MqttConnectionSettings connectionSettings;

        private TaskCompletionSource<SessionState> taskCompletionSource;

        public ConnectionComponent(IMessageSender messageSender, MqttConnectionSettings connectionSettings)
        {            
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            this.connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }

        public async Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken cancellationToken = default)
        {
            await messageSender.SendAsync(TransportCommand.Open, cancellationToken);

            if (connectionSettings.UseTls)
            {
                await messageSender.SendAsync(TransportCommand.SetTransportLevelSecurity, cancellationToken);
            }

            var connectPacket = CreateConnectPacket(cleanSession);
            await messageSender.TransmitAsync(connectPacket, cancellationToken);

            taskCompletionSource = new TaskCompletionSource<SessionState>();

            return await taskCompletionSource.Task;
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await messageSender.TransmitAsync(Disconnect.Instance, cancellationToken);
            await messageSender.SendAsync(TransportCommand.Close, cancellationToken);
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

        public void Handle(ConnAck connAck)
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
