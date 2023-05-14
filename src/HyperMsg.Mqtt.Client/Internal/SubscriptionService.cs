using HyperMsg.Mqtt.Packets;
using System.Collections.Concurrent;

namespace HyperMsg.Mqtt.Client.Internal;

public class SubscriptionService : Service
{
    private readonly ConcurrentDictionary<ushort, Subscribe> requestedSubscriptions;
    private readonly ConcurrentDictionary<ushort, Unsubscribe> requestedUnsubscriptions;

    public SubscriptionService(ITopic messageTopic) : base(messageTopic)
    {
        requestedSubscriptions = new();
        requestedUnsubscriptions = new();
    }

    public IReadOnlyDictionary<ushort, Subscribe> PendingSubscriptionRequests => requestedSubscriptions;

    public IReadOnlyDictionary<ushort, Unsubscribe> PendingUnsubscriptionRequests => requestedUnsubscriptions;

    protected override void RegisterHandlers(IRegistry registry)
    {
        registry.Register<SubAck>(HandleSubscriptionResponse);
        registry.Register<UnsubAck>(HandleUnsubAckResponse);
    }

    protected override void UnregisterHandlers(IRegistry registry)
    {
        registry.Unregister<SubAck>(HandleSubscriptionResponse);
        registry.Unregister<UnsubAck>(HandleUnsubAckResponse);
    }

    public ushort RequestSubscription(params SubscriptionRequest[] subscriptions) => RequestSubscription((IEnumerable<SubscriptionRequest>)subscriptions);

    public ushort RequestSubscription(IEnumerable<SubscriptionRequest> subscriptions)
    {
        //var request = new Subscribe(PacketId.New(), subscriptions);
        //requestedSubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);
        //Dispatch(request);
        //return request.Id;
        return default;
    }

    public ushort RequestUnsubscription(params string[] topicNames) => RequestUnsubscription((IEnumerable<string>)topicNames);

    public ushort RequestUnsubscription(IEnumerable<string> topicNames)
    {
        var request = new Unsubscribe(PacketId.New(), topicNames);
        requestedUnsubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);
        Dispatch(request);
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
