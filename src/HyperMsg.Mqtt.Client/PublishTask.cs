using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class PublishTask : MessagingTask
    {
        private readonly PublishRequest request;
        private readonly ushort packetId;

        internal PublishTask(IMessagingContext messagingContext, PublishRequest request, CancellationToken cancellationToken) : base(messagingContext, cancellationToken)
        {
            this.request = request;
            packetId = PacketId.New();
        }

        internal async Task<PublishTask> StartAsync()
        {
            if (request.Qos == QosLevel.Qos1)
            {
                RegisterReceiveHandler<PubAck>(Handle);
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
    }
}
