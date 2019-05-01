using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RequestDictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, System.Threading.Tasks.TaskCompletionSource<System.Collections.Generic.IEnumerable<HyperMsg.Mqtt.SubscriptionResult>>>;

namespace HyperMsg.Mqtt.Client
{
    internal class SubscriptionHandler
    {
        private readonly RequestDictionary pendingRequests;
        private readonly ISender<Packet> sender;

        internal SubscriptionHandler(ISender<Packet> sender)
        {
            this.sender = sender;
            pendingRequests = new RequestDictionary();
        }

        internal async Task<IEnumerable<SubscriptionResult>> SendSubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken token)
        {
            var request = CreateSubscribeRequest(requests);
            await sender.SendAsync(request, token);

            var tsc = new TaskCompletionSource<IEnumerable<SubscriptionResult>>();
            pendingRequests.AddOrUpdate(request.Id, tsc, (k, v) => v);
            return await tsc.Task;
        }

        private Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests)
        {
            return new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));
        }

        internal void OnSubAckReceived(SubAck subAck)
        {
            if (pendingRequests.TryRemove(subAck.Id, out var tsc))
            {
                tsc.SetResult(subAck.Results);
            }
        }
    }
}
