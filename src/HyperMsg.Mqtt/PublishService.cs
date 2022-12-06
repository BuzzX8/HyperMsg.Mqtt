using HyperMsg.Mqtt.Packets;
using System.Collections.Concurrent;

namespace HyperMsg.Mqtt;

public class PublishService : Service
{
    private readonly ConcurrentDictionary<ushort, Publish> pendingPublications;

    public PublishService(ITopic messageTopic) : base(messageTopic)
    {
        pendingPublications = new();
    }

    public IReadOnlyDictionary<ushort, Publish> PendingPublications => pendingPublications;

    public ushort Publish(PublishRequest publishRequest)
    {
        var publish = new Publish(PacketId.New(), publishRequest.TopicName, publishRequest.Message, publishRequest.Qos);
        return DispatchPublish(publish);
    }

    public ushort Publish(string topicName, ReadOnlyMemory<byte> message, QosLevel qos)
    {
        var publish = new Publish(PacketId.New(), topicName, message, qos);
        return DispatchPublish(publish);
    }

    private ushort DispatchPublish(Publish publish)
    {
        if (publish.Qos != QosLevel.Qos0)
        {
            pendingPublications.TryAdd(publish.Id, publish);
        }

        Dispatch(publish);
        return publish.Id;
    }

    private void HandlePubAckResponse(PubAck pubAck)
    {
        if (pendingPublications.TryGetValue(pubAck.Id, out var publish) && publish.Qos == QosLevel.Qos1)
        {
            pendingPublications.Remove(pubAck.Id, out _);
        }
    }

    private void HandlePubRecResponse(PubRec pubRec)
    {
        if (!pendingPublications.TryGetValue(pubRec.Id, out var publish) && publish.Qos != QosLevel.Qos2)
        {
            return;
        }

        Dispatch(new PubRel(pubRec.Id));
        //requestStorage.AddOrReplace(pubRec.Id, pubRec);
    }

    private void HandlePubCompResponse(PubComp pubComp)
    {
        //if (!requestStorage.Contains<PubRec>(pubComp.Id) && !requestStorage.Contains<Publish>(pubComp.Id))
        //{
        //    return;
        //}

        //var publish = requestStorage.Get<Publish>(pubComp.Id);

        //if (publish.Qos != QosLevel.Qos2)
        //{
        //    return;
        //}

        //dispatcher.Dispatch(new PublishCompletedHandlerArgs(publish.Id, publish.Topic, publish.Qos));
        //requestStorage.Remove<Publish>(publish.Id);
        //requestStorage.Remove<PubRec>(publish.Id);
    }
    protected override void RegisterHandlers(IRegistry registry)
    {
        registry.Register<PubAck>(HandlePubAckResponse);
        registry.Register<PubRec>(HandlePubRecResponse);
        registry.Register<PubComp>(HandlePubCompResponse);
    }

    protected override void UnregisterHandlers(IRegistry registry)
    {
        registry.Unregister<PubAck>(HandlePubAckResponse);
        registry.Unregister<PubRec>(HandlePubRecResponse);
        registry.Unregister<PubComp>(HandlePubCompResponse);
    }
}
