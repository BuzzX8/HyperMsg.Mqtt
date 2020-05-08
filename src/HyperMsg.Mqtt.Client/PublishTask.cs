using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class PublishTask : MessagingTask
    {
        private readonly PublishRequest request;
        private readonly ushort packetId;

        private bool pubRecReceived;

        internal PublishTask(IMessagingContext messagingContext, PublishRequest request, CancellationToken cancellationToken) : base(messagingContext, cancellationToken)
        {
            this.request = request;
            packetId = PacketId.New();
        }

        internal async Task<PublishTask> RunAsync()
        {
            if (request.Qos == QosLevel.Qos1)
            {
                RegisterReceiveHandler<PubAck>(Handle);
            }

            if (request.Qos == QosLevel.Qos2)
            {
                RegisterReceiveHandler<PubRec>(HandleAsync);
                RegisterReceiveHandler<PubComp>(Handle);
            }

            await Sender.TransmitPublishAsync(packetId, request, CancellationToken);

            if (request.Qos == QosLevel.Qos0)
            {
                SetCompleted();
            }

            return this;
        }

        private void Handle(PubAck pubAck)
        {
            if (pubAck.Id != packetId)
            {
                return;
            }

            SetCompleted();
        }

        private async Task HandleAsync(PubRec pubRec, CancellationToken cancellationToken)
        {
            if (pubRec.Id != packetId)
            {
                return;
            }

            pubRecReceived = true;
            await Sender.TransmitAsync(new PubRel(packetId), CancellationToken);
        }

        private void Handle(PubComp pubComp)
        {
            if (pubComp.Id != packetId || !pubRecReceived)
            {
                return;
            }

            SetCompleted();
        }
    }
}
