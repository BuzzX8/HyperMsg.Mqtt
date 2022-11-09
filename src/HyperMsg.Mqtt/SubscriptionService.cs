using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HyperMsg.Mqtt;

public class SubscriptionService : IDisposable
{
    private readonly IDispatcher dispatcher;
    private readonly IRegistry registry;
    private readonly ConcurrentDictionary<ushort, Subscribe> requestedSubscriptions;

    public SubscriptionService(IDispatcher dispatcher, IRegistry registry)
    {
        this.dispatcher = dispatcher;
        this.registry = registry;
        requestedSubscriptions = new();
        RegisterHandlers(this.registry);
    }

    private void RegisterHandlers(IRegistry registry)
    {
        registry.Register<SubAck>(HandleSubscriptionResponse);
        registry.Register<UnsubAck>(HandleUnsubAckResponse);
    }

    private void DeregisterHandlers(IRegistry registry)
    {
        registry.Deregister<SubAck>(HandleSubscriptionResponse);
        registry.Deregister<UnsubAck>(HandleUnsubAckResponse);
    }

    public ushort RequestSubscription(params SubscriptionRequest[] subscriptions) => RequestSubscription((IEnumerable<SubscriptionRequest>)subscriptions);

    public ushort RequestSubscription(IEnumerable<SubscriptionRequest> subscriptions)
    {
        var request = new Subscribe(PacketId.New(), subscriptions);
        requestedSubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);
        dispatcher.Dispatch(request);
        return request.Id;
    }

    public ushort RequestUnsubscription(params string[] topicNames) => RequestUnsubscription((IEnumerable<string>)topicNames);

    public ushort RequestUnsubscription(IEnumerable<string> topicNames)
    {
        var request = new Unsubscribe(PacketId.New(), topicNames);

        dispatcher.Dispatch(request);
        return request.Id;
    }

    private void HandleSubscriptionResponse(SubAck response)
    {
        if (!requestedSubscriptions.TryGetValue(response.Id, out var request))
        {
            return;
        }

        var requestedTopics = request.Subscriptions.Select(s => s.TopicName).ToArray();
                
        requestedSubscriptions.Remove(response.Id, out _);
    }

    private void HandleUnsubAckResponse(UnsubAck unsubAck)
    {
        
    }

    public void Dispose() => DeregisterHandlers(registry);
}
