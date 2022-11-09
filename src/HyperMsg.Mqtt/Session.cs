using HyperMsg.Mqtt.Packets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HyperMsg.Mqtt;

public class Session
{
    private readonly IDispatcher dispatcher;
    private readonly ConcurrentDictionary<ushort, Subscribe> requestedSubscriptions;
    private readonly RequestStorage requestStorage;

    public Session()
    {
        requestedSubscriptions = new();
    }

    public void RequestSubscription(IEnumerable<SubscriptionRequest> subscriptions)
    {
        var request = new Subscribe(PacketId.New(), subscriptions);
        requestedSubscriptions.AddOrUpdate(request.Id, request, (id, val) => val);
        dispatcher.Dispatch(request);
    }

    private void HandleSubscriptionResponse(ushort responseId)
    {
        if (!requestedSubscriptions.TryGetValue(responseId, out var request))
        {
            return;
        }

        var requestedTopics = request.Subscriptions.Select(s => s.TopicName).ToArray();
        //dispatcher.Dispatch(new SubscriptionResponseHandlerArgs(requestedTopics, subAck.Results.ToArray()));

        //requestStorage.Remove<Subscribe>(subAck.Id);
        requestedSubscriptions.Remove(responseId, out _);
    }

    private void HandleUnsubscribeRequest(Unsubscribe unsubscribe) => requestStorage.AddOrReplace(unsubscribe.Id, unsubscribe);

    private void HandleUnsubAckResponse(UnsubAck unsubAck)
    {
        if (!requestStorage.TryGet<Unsubscribe>(unsubAck.Id, out var request))
        {
            return;
        }

        requestStorage.Remove<Unsubscribe>(unsubAck.Id);
    }

    private void HandlePublishRequest(Publish publish)
    {
        if (publish.Qos == QosLevel.Qos0)
        {
            return;
        }

        requestStorage.AddOrReplace(publish.Id, publish);
    }

    private void HandlePubAckResponseAsync(PubAck pubAck)
    {
        if (requestStorage.TryGet<Publish>(pubAck.Id, out var publish) && publish.Qos == QosLevel.Qos1)
        {
            //dispatcher.Dispatch(new PublishCompletedHandlerArgs(publish.Id, publish.Topic, publish.Qos));
            requestStorage.Remove<Publish>(publish.Id);
        }
    }

    private void HandlePubRecResponseAsync(PubRec pubRec)
    {
        if (!requestStorage.TryGet<Publish>(pubRec.Id, out var publish) && publish.Qos != QosLevel.Qos2)
        {
            return;
        }

        //dispatcher.Dispatch(new PubRel(pubRec.Id));
        requestStorage.AddOrReplace(pubRec.Id, pubRec);
    }

    private void HandlePubCompResponseAsync(PubComp pubComp)
    {
        if (!requestStorage.Contains<PubRec>(pubComp.Id) && !requestStorage.Contains<Publish>(pubComp.Id))
        {
            return;
        }

        var publish = requestStorage.Get<Publish>(pubComp.Id);

        if (publish.Qos != QosLevel.Qos2)
        {
            return;
        }

        //dispatcher.Dispatch(new PublishCompletedHandlerArgs(publish.Id, publish.Topic, publish.Qos));
        requestStorage.Remove<Publish>(publish.Id);
        requestStorage.Remove<PubRec>(publish.Id);
    }
}
