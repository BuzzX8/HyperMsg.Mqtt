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

    public async Task<ushort> RequestSubscriptionAsync(IEnumerable<SubscriptionRequest> subscriptions, CancellationToken cancellationToken = default)
    {
        var request = new Subscribe(PacketId.New(), subscriptions);
        requestedSubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);
        
        await sendAction.Invoke(request, cancellationToken);

        return request.Id;
    }

    public ushort RequestUnsubscription(params string[] topicNames) => RequestUnsubscription((IEnumerable<string>)topicNames);

    public ushort RequestUnsubscription(IEnumerable<string> topicNames)
    {
        var request = new Unsubscribe(PacketId.New(), topicNames);
        requestedUnsubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);
        //Dispatch(request);
        return request.Id;
    }

    private void HandleSubscriptionResponse(SubAck response)
    {
        if (!requestedSubscriptions.TryGetValue(response.Id, out var request))
        {
            return;
        }

        //var requestedTopics = request.Subscriptions.Select(s => s.TopicName).ToArray();

        requestedSubscriptions.Remove(response.Id, out _);
    }

    private void HandleUnsubAckResponse(UnsubAck unsubAck)
    {
        if (!requestedUnsubscriptions.ContainsKey(unsubAck.Id))
        {
            return;
        }

        requestedUnsubscriptions.Remove(unsubAck.Id, out _);
    }
}
