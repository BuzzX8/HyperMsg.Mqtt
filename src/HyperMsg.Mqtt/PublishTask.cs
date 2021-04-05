using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class PublishTask : MessagingTask<bool>
    {
        private QosLevel qos;
        private ushort packetId;

        public PublishTask(IMessagingContext context, CancellationToken cancellationToken = default) : base(context, cancellationToken)
        {
            this.RegisterMessageReceivedEventHandler<PubAck>(Handle);
            this.RegisterMessageReceivedEventHandler<PubRec>(HandleAsync);
            this.RegisterMessageReceivedEventHandler<PubComp>(Handle);
        }

        public async Task<MessagingTask<bool>> StartAsync(PublishRequest request)
        {
            var publishPacket = CreatePublishPacket(request);
            packetId = publishPacket.Id;
            await this.SendTransmitMessageCommandAsync(publishPacket, CancellationToken);

            if (request.Qos == QosLevel.Qos0)
            {
                SetResult(true);
                return this;
            }

            qos = request.Qos;

            return this;
        }

        private Publish CreatePublishPacket(PublishRequest request) => new Publish(PacketId.New(), request.TopicName, request.Message, request.Qos);

        private void Handle(PubAck pubAck)
        {
            if (pubAck.Id != packetId && qos == QosLevel.Qos1)
            {
                return;
            }

            SetResult(true);
        }

        private async Task HandleAsync(PubRec pubRec, CancellationToken cancellationToken)
        {
            if (pubRec.Id != packetId && qos == QosLevel.Qos2)
            {
                return;
            }

            await this.SendTransmitMessageCommandAsync(new PubRel(packetId), cancellationToken);
        }

        private void Handle(PubComp pubComp)
        {
            if (pubComp.Id != packetId && qos == QosLevel.Qos2)
            {
                return;
            }

            SetResult(true);
        }
    }
}
