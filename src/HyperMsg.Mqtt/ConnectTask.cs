using HyperMsg.Mqtt.Packets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using HyperMsg.Transport;

namespace HyperMsg.Mqtt
{
    internal class ConnectTask : MessagingTask<SessionState>
    {
        private readonly MqttConnectionSettings connectionSettings;

        private ConnectTask(IMessagingContext context, MqttConnectionSettings connectionSettings) : base(context)
        {
            this.connectionSettings = connectionSettings;
        }

        public static ConnectTask StartNew(IMessagingContext context, MqttConnectionSettings connectionSettings)
        {
            var task = new ConnectTask(context, connectionSettings);
            task.Start();
            return task;
        }

        protected override async Task BeginAsync()
        {
            await this.SendOpenConnectionCommandAsync();

            if (connectionSettings.UseTls)
            {
                await this.SendSetTlsCommandAsync();
            }

            var connectPacket = CreateConnectPacket(connectionSettings);
            await this.SendTransmitMessageCommandAsync(connectPacket);
        }

        private static Connect CreateConnectPacket(MqttConnectionSettings connectionSettings)
        {
            var flags = ConnectFlags.None;

            if (connectionSettings.CleanSession)
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

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<ConnAck>(Handle);
        }

        private void Handle(ConnAck connAck) => SetResult(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
    }
}
