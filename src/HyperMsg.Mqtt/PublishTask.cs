using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class PublishTask : MessagingTask
    {
        private readonly PublishRequest request;
        private QosLevel qos;
        private ushort packetId;

        public PublishTask(IMessagingContext context, PublishRequest request) : base(context) => this.request = request;

        public static PublishTask StartNew(IMessagingContext context, PublishRequest request)
        {
            var task = new PublishTask(context, request);
            task.Start();
            return task;
        }

        protected override async Task BeginAsync()
        {
            var publishPacket = CreatePublishPacket(request);
            packetId = publishPacket.Id;
            await this.SendTransmitMessageCommandAsync(publishPacket);

            if (request.Qos == QosLevel.Qos0)
            {
                SetCompleted();
                return;
            }

            qos = request.Qos;

            return;
        }

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<PubAck>(Handle);
            yield return this.RegisterMessageReceivedEventHandler<PubRec>(HandleAsync);
            yield return this.RegisterMessageReceivedEventHandler<PubComp>(Handle);
        }

        private Publish CreatePublishPacket(PublishRequest request) => new Publish(PacketId.New(), request.TopicName, request.Message, request.Qos);

        private void Handle(PubAck pubAck)
        {
            if (pubAck.Id != packetId && qos == QosLevel.Qos1)
            {
                return;
            }

            SetCompleted();
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

            SetCompleted();
        }
    }
}
