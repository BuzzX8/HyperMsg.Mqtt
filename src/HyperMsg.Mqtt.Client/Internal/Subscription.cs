using HyperMsg.Mqtt.Packets;
using System.Collections.Concurrent;
using SendAction = System.Func<HyperMsg.Mqtt.Packets.Packet, System.Threading.CancellationToken, System.Threading.Tasks.Task>;

namespace HyperMsg.Mqtt.Client.Internal;

public class Subscription
{
    private readonly ConcurrentDictionary<ushort, Subscribe> requestedSubscriptions;
    private readonly ConcurrentDictionary<ushort, Unsubscribe> requestedUnsubscriptions;

    private readonly SendAction sendAction;

    public Subscription(SendAction sendAction)
    {
        ArgumentNullException.ThrowIfNull(sendAction, nameof(sendAction));

        this.sendAction = sendAction;

        requestedSubscriptions = new();
        requestedUnsubscriptions = new();
    }

    public IReadOnlyDictionary<ushort, Subscribe> PendingSubscriptionRequests => requestedSubscriptions;

    public IReadOnlyDictionary<ushort, Unsubscribe> PendingUnsubscriptionRequests => requestedUnsubscriptions;

    public async Task<ushort> RequestSubscriptionAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default)
    {
        var request = new Subscribe(PacketId.New(), requests);
        requestedSubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);

        await sendAction.Invoke(request, cancellationToken);

        return request.Id;
    }

    public async Task<ushort> RequestUnsubscriptionAsync(IEnumerable<string> topicFilters, CancellationToken cancellationToken = default)
    {
        var request = new Unsubscribe(PacketId.New(), topicFilters);
        requestedUnsubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);

        await sendAction.Invoke(request, default);

        return request.Id;
    }

    private void HandleSubAck(SubAck response)
    {
        if (!requestedSubscriptions.TryGetValue(response.Id, out var request))
        {
            return;
        }

        //var requestedTopics = request.Subscriptions.Select(s => s.TopicName).ToArray();

        requestedSubscriptions.Remove(response.Id, out _);
    }

    private void HandleUnsubAck(UnsubAck unsubAck)
    {
        if (!requestedUnsubscriptions.ContainsKey(unsubAck.Id))
        {
            return;
        }

        requestedUnsubscriptions.Remove(unsubAck.Id, out _);
    }
}
