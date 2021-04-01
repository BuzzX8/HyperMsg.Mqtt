using HyperMsg.Mqtt.Packets;
using System.Threading;
using System.Threading.Tasks;
using HyperMsg.Mqtt.Extensions;
using HyperMsg.Extensions;
using HyperMsg.Transport.Extensions;
using System.Collections.Generic;
using System;

namespace HyperMsg.Mqtt
{
    internal class ConnectTask : MessagingTask<SessionState>
    {
        private readonly MqttConnectionSettings connectionSettings;

        public ConnectTask(IMessagingContext context, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken) : base(context, cancellationToken)
        {
            this.connectionSettings = connectionSettings;
        }

        //internal new ConnectTask Start() => base.Start();

        protected override async Task BeginAsync()
        {
            await this.SendOpenConnectionCommandAsync(CancellationToken);

            if (connectionSettings.UseTls)
            {
                await this.SendSetTlsCommandAsync(CancellationToken);
            }

            await this.TransmitConnectionRequestAsync(connectionSettings, CancellationToken);
        }

        protected override IEnumerable<IDisposable> GetDefaultDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<ConnAck>(Handle);
        }

        private void Handle(ConnAck connAck) => SetResult(connAck.SessionPresent ? SessionState.Present : SessionState.Clean);
    }
}
