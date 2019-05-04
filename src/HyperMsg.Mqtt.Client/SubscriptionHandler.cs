﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RequestDictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, System.Threading.Tasks.TaskCompletionSource<System.Collections.Generic.IEnumerable<HyperMsg.Mqtt.SubscriptionResult>>>;
using UnsubscribeDictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, System.Threading.Tasks.TaskCompletionSource<bool>>;

namespace HyperMsg.Mqtt.Client
{
    internal class SubscriptionHandler
    {
        private readonly RequestDictionary pendingRequests;
        private readonly UnsubscribeDictionary unsubscribeDictionary;
        private readonly ISender<Packet> sender;

        internal SubscriptionHandler(ISender<Packet> sender)
        {
            this.sender = sender;
            pendingRequests = new RequestDictionary();
            unsubscribeDictionary = new UnsubscribeDictionary();
        }

        internal async Task<IEnumerable<SubscriptionResult>> SendSubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken token)
        {
            var request = CreateSubscribeRequest(requests);
            await sender.SendAsync(request, token);

            var tsc = new TaskCompletionSource<IEnumerable<SubscriptionResult>>();
            pendingRequests.AddOrUpdate(request.Id, tsc, (k, v) => v);
            return await tsc.Task;
        }

        private Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests) => new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));

        internal async Task SendUnsubscribeAsync(IEnumerable<string> topics, CancellationToken token)
        {
            var request = CreateUnsubscribeRequest(topics);
            await sender.SendAsync(request, token);

            var tsc = new TaskCompletionSource<bool>();
            unsubscribeDictionary.AddOrUpdate(request.Id, tsc, (k, v) => v);
            await tsc.Task;
        }

        private Unsubscribe CreateUnsubscribeRequest(IEnumerable<string> topics) => new Unsubscribe(PacketId.New(), topics);

        internal void OnSubAckReceived(SubAck subAck)
        {
            if (pendingRequests.TryRemove(subAck.Id, out var tsc))
            {
                tsc.SetResult(subAck.Results);
            }
        }

        internal void OnUnsubAckReceived(UnsubAck unsubAck)
        {
            if (unsubscribeDictionary.TryRemove(unsubAck.Id, out var tsc))
            {
                tsc.SetResult(true);
            }
        }
    }
}
