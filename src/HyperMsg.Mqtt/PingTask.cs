using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public class PingTask : MessagingTask<bool>
    {
        private PingTask(IMessagingContext context, CancellationToken cancellationToken) : base(context, cancellationToken)
        { }

        internal static PingTask StartNew(IMessagingContext context, CancellationToken cancellationToken)
        {
            var task = new PingTask(context, cancellationToken);
            task.Start();
            return task;
        }

        protected override Task BeginAsync()
        {
            return this.SendTransmitMessageCommandAsync(PingReq.Instance, CancellationToken);
        }

        protected override IEnumerable<IDisposable> GetDefaultDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<PingResp>(Handle);
        }

        private void Handle(PingResp _) => SetResult(true);
    }
}
