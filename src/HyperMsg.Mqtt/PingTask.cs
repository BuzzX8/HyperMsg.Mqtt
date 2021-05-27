using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public class PingTask : MessagingTask
    {
        private PingTask(IMessagingContext context) : base(context)
        { }

        internal static PingTask StartNew(IMessagingContext context)
        {
            var task = new PingTask(context);
            task.Start();
            return task;
        }

        protected override Task BeginAsync()
        {
            return this.SendTransmitMessageCommandAsync(PingReq.Instance);
        }

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<PingResp>(Handle);
        }

        private void Handle(PingResp _) => SetCompleted();
    }
}
