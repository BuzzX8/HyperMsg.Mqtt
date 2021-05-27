using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class UnsubscribeTask : MessagingTask
    {
        private readonly IEnumerable<string> topics;
        private ushort packetId;

        internal UnsubscribeTask(IEnumerable<string> topics, IMessagingContext context) : base(context) => this.topics = topics;

        public static UnsubscribeTask StartNew(IEnumerable<string> topics, IMessagingContext messagingContext)
        {
            var task = new UnsubscribeTask(topics, messagingContext);
            task.Start();
            return task;
        }

        protected override Task BeginAsync()
        {
            var request = CreateUnsubscribeRequest(topics);
            packetId = request.Id;
            return this.SendTransmitMessageCommandAsync(request);
        }

        private Unsubscribe CreateUnsubscribeRequest(IEnumerable<string> topics) => new Unsubscribe(PacketId.New(), topics);

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<UnsubAck>(Handle);
        }

        private void Handle(UnsubAck unsubAck)
        {
            if (unsubAck.Id == packetId)
            {
                SetCompleted();
            }
        }
    }
}
