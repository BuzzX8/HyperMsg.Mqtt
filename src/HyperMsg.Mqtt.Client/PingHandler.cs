using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    internal class PingHandler
    {
        private readonly ISender<Packet> sender;
        private TaskCompletionSource<bool> pingTsc;

        internal PingHandler(ISender<Packet> sender)
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

        internal void OnPingRespReceived()
        {
            if (pingTsc != null)
            {
                pingTsc.SetResult(true);
                pingTsc = null;
            }
        }
    }
}
