using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class PingTask : MessagingTask<bool>
    {
        public PingTask(IMessagingContext context, CancellationToken cancellationToken = default) : base(context, cancellationToken)
        {
            this.RegisterReceiveHandler<PingResp>(Handle);
        }

        internal async Task<MessagingTask<bool>> StartAsync()
        {
            await this.TransmitAsync(PingReq.Instance, CancellationToken);

            return this;
        }

        private void Handle(PingResp _) => Complete(true);
    }
}
