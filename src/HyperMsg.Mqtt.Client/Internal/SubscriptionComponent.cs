using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RequestDictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, System.Threading.Tasks.TaskCompletionSource<System.Collections.Generic.IEnumerable<HyperMsg.Mqtt.SubscriptionResult>>>;
using UnsubscribeDictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, System.Threading.Tasks.TaskCompletionSource<bool>>;

namespace HyperMsg.Mqtt.Client
{
    public class SubscriptionComponent
    {
        private readonly RequestDictionary pendingRequests;
        private readonly UnsubscribeDictionary unsubscribeDictionary;
        private readonly IMessageSender messageSender;

        public SubscriptionComponent(IMessageSender messageSender)
        {
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            pendingRequests = new RequestDictionary();
            unsubscribeDictionary = new UnsubscribeDictionary();
        }

        public async Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken)
        {
            var request = CreateSubscribeRequest(requests);
            await messageSender.TransmitAsync(request, cancellationToken);

            var tsc = new TaskCompletionSource<IEnumerable<SubscriptionResult>>();
            pendingRequests.AddOrUpdate(request.Id, tsc, (k, v) => v);
            return await tsc.Task;
        }

        private Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests) => new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));

        public async Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken token)
        {
            var request = CreateUnsubscribeRequest(topics);
            await messageSender.TransmitAsync(request, token);

            var tsc = new TaskCompletionSource<bool>();
            unsubscribeDictionary.AddOrUpdate(request.Id, tsc, (k, v) => v);
            await tsc.Task;
        }

        private Unsubscribe CreateUnsubscribeRequest(IEnumerable<string> topics) => new Unsubscribe(PacketId.New(), topics);

        public void Handle(SubAck subAck)
        {
            if (pendingRequests.TryRemove(subAck.Id, out var tsc))
            {
                tsc.SetResult(subAck.Results);
            }
        }

        public void Handle(UnsubAck unsubAck)
        {
            if (unsubscribeDictionary.TryRemove(unsubAck.Id, out var tsc))
            {
                tsc.SetResult(true);
            }
        }
    }
}
