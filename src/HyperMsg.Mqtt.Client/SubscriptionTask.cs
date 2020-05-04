using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class SubscriptionTask : MessagingTask<IEnumerable<SubscriptionResult>>
    {
        private readonly IEnumerable<(string, QosLevel)> requests;
        private readonly ushort packetId;

        internal SubscriptionTask(IMessagingContext messagingContext, IEnumerable<(string, QosLevel)> requests, CancellationToken cancellationToken) : base(messagingContext, cancellationToken)
        {
            this.requests = requests;
            packetId = PacketId.New();
        }
        
        internal async Task<SubscriptionTask> RunAsync()
        {
            RegisterReceiveHandler<SubAck>(Handle);
            await Sender.TransmitSubscribeAsync(packetId, requests, CancellationToken);
            return this;
        }

        private void Handle(SubAck subAck)
        {
            if (subAck.Id != packetId)
            {
                return;
            }

            SetResult(subAck.Results);
        }
    }
}
