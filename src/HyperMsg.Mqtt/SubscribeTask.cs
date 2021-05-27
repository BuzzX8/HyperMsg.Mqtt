using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public class SubscribeTask : MessagingTask<IEnumerable<SubscriptionResult>>
    {
        private readonly IEnumerable<SubscriptionRequest> requests;
        private ushort packetId;

        private SubscribeTask(IEnumerable<SubscriptionRequest> requests, IMessagingContext context, CancellationToken cancellationToken) : base(context, cancellationToken)
        {
            this.requests = requests;
        }

        public static SubscribeTask StartNew(IEnumerable<SubscriptionRequest> requests, IMessagingContext context, CancellationToken cancellationToken = default)
        {
            var task = new SubscribeTask(requests, context, cancellationToken);
            task.Start();
            return task;
        }

        protected override IEnumerable<IDisposable> GetDefaultDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<SubAck>(Handle);
        }

        protected override Task BeginAsync()
        {
            var request = CreateSubscribeRequest(requests);
            packetId = request.Id;
            
            return this.SendTransmitMessageCommandAsync(request, CancellationToken);
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
