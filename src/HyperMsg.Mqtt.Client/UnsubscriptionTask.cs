using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class UnsubscriptionTask : MessagingTask
    {
        private readonly IEnumerable<string> topics;
        private readonly ushort packetId;

        public UnsubscriptionTask(IMessagingContext messagingContext, IEnumerable<string> topics, CancellationToken cancellationToken) : base(messagingContext, cancellationToken)
        {
            this.topics = topics;
            packetId = PacketId.New();
        }

        internal async Task<UnsubscriptionTask> RunAsync()
        {
            RegisterReceiveHandler<UnsubAck>(Handle);
            await Sender.TransmitUnsubscribeAsync(packetId, topics, CancellationToken);
            return this;
        }

        private void Handle(UnsubAck unsubAck)
        {
            if (unsubAck.Id != packetId)
            {
                return;
            }

            SetCompleted();
        }
    }
}