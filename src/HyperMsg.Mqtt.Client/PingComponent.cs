using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class PingComponent
    {
        private readonly IMessageSender<Packet> sender;
        private TaskCompletionSource<bool> pingTsc;

        public PingComponent(IMessageSender<Packet> sender)
        {
            this.sender = sender;
        }

        public async Task PingAsync(CancellationToken cancellationToken)
        {
            if (pingTsc != null)
            {
                return;
            }

            await sender.SendAsync(PingReq.Instance, cancellationToken);
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
