using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    internal class PingHandler
    {
        private readonly IMessageSender<Packet> sender;
        private TaskCompletionSource<bool> pingTsc;

        internal PingHandler(IMessageSender<Packet> sender)
        {
            this.sender = sender;
        }

        internal async Task SendPingReqAsync(CancellationToken token)
        {
            if (pingTsc != null)
            {
                return;
            }

            await sender.SendAsync(PingReq.Instance, token);
            pingTsc = new TaskCompletionSource<bool>();
            await pingTsc.Task;
        }

        internal void Handle()
        {
            if (pingTsc != null)
            {
                pingTsc.SetResult(true);
                pingTsc = null;
            }
        }
    }
}
