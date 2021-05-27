using HyperMsg.Mqtt.Packets;
using System.Threading;
using System.Threading.Tasks;
using HyperMsg.Mqtt.Extensions;
using System.Collections.Generic;
using System;
using HyperMsg.Transport;

namespace HyperMsg.Mqtt
{
    public class ConnectTask : MessagingTask<SessionState>
    {
        private readonly MqttConnectionSettings connectionSettings;

        private ConnectTask(IMessagingContext context, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken) : base(context)
        {
            this.connectionSettings = connectionSettings;
        }

        public static ConnectTask StartNew(IMessagingContext context, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            var task = new ConnectTask(context, connectionSettings, cancellationToken);
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

            await this.TransmitConnectionRequestAsync(connectionSettings);
        }

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<ConnAck>(Handle);
        }

        private void Handle(ConnAck connAck) => SetResult(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
    }
}
