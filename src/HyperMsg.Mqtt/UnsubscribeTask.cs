using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class UnsubscribeTask : MessagingTask<bool>
    {
        private ushort packetId;

        internal UnsubscribeTask(IMessagingContext context, CancellationToken cancellationToken = default) : base(context, cancellationToken)
        {
            this.RegisterReceiveHandler<UnsubAck>(Handle);
        }

        public async Task<MessagingTask<bool>> StartAsync(IEnumerable<string> topics)
        {
            var request = CreateUnsubscribeRequest(topics);
            packetId = request.Id;
            await this.TransmitAsync(request, CancellationToken);
            
            return this;
        }

        private Unsubscribe CreateUnsubscribeRequest(IEnumerable<string> topics) => new Unsubscribe(PacketId.New(), topics);

        private void Handle(UnsubAck unsubAck)
        {
            if (unsubAck.Id == packetId)
            {
                Complete(true);
            }
        }
    }
}
