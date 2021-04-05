using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class SubscribeTask : MessagingTask<IEnumerable<SubscriptionResult>>
    {
        private ushort packetId;

        internal SubscribeTask(IMessagingContext context, CancellationToken cancellationToken = default) : base(context, cancellationToken)
        {
            this.RegisterMessageReceivedEventHandler<SubAck>(Handle);
        }

        internal async Task<MessagingTask<IEnumerable<SubscriptionResult>>> StartAsync(IEnumerable<SubscriptionRequest> requests)
        {
            var request = CreateSubscribeRequest(requests);
            packetId = request.Id;
            await this.SendTransmitMessageCommandAsync(request, CancellationToken);            
            
            return this;
        }

        private Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests) => new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));

        private void Handle(SubAck subAck)
        {
            if (subAck.Id == packetId)
            {
                SetResult(subAck.Results);
            }
        }
    }
}
