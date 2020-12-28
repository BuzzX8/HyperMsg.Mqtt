using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public class PingComponent
    {
        private readonly IMessageSender messageSender;
        private TaskCompletionSource<bool> pingTsc;

        public PingComponent(IMessageSender messageSender)
        {
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        public async Task<Task> PingAsync(CancellationToken cancellationToken)
        {
            if (pingTsc == null)
            {
                await messageSender.TransmitAsync(PingReq.Instance, cancellationToken);
                return (pingTsc = new TaskCompletionSource<bool>()).Task;
            }

            return pingTsc.Task;
        }

        public void Handle(PingResp _)
        {
            if (pingTsc != null)
            {
                pingTsc.SetResult(true);
                pingTsc = null;
            }
        }
    }
}
