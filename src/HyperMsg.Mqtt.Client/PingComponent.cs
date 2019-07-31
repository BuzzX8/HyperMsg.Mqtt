using System;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class PingComponent
    {
        private readonly IMessageSender<Packet> messageSender;
        private TaskCompletionSource<bool> pingTsc;

        public PingComponent(IMessageSender<Packet> messageSender)
        {
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        public async Task PingAsync(CancellationToken cancellationToken)
        {
            if (pingTsc != null)
            {
                return;
            }

            await messageSender.SendAsync(PingReq.Instance, cancellationToken);
            pingTsc = new TaskCompletionSource<bool>();
            await pingTsc.Task;
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
