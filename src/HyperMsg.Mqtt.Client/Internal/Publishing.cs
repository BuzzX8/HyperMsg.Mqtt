using HyperMsg.Mqtt.Packets;
using System.Collections.Concurrent;
using SendAction = System.Func<HyperMsg.Mqtt.Packets.Packet, System.Threading.CancellationToken, System.Threading.Tasks.Task>;

namespace HyperMsg.Mqtt.Client.Internal;

public class Publishing
{
    private readonly ConcurrentDictionary<ushort, Publish> pendingPublications;
    private readonly ConcurrentBag<ushort> releasedPublications;

    private readonly SendAction sendAction;

    public Publishing(SendAction sendAction)
    {
        ArgumentNullException.ThrowIfNull(sendAction, nameof(sendAction));
        this.sendAction = sendAction;

        pendingPublications = new();
        releasedPublications = new();
    }

    public IReadOnlyDictionary<ushort, Publish> PendingPublications => pendingPublications;

    public IReadOnlyCollection<ushort> ReleasedPublications => releasedPublications;

    public Task<ushort> PublishAsync(PublishRequest publishRequest, CancellationToken cancellationToken = default)
    {
        var publish = new Publish(PacketId.New(), publishRequest.TopicName, publishRequest.Message, publishRequest.Qos);

        return DispatchPublish(publish, cancellationToken);
    }

    public Task<ushort> PublishAsync(string topicName, ReadOnlyMemory<byte> message, QosLevel qos, CancellationToken cancellationToken = default)
    {
        var publish = new Publish(PacketId.New(), topicName, message, qos);

        return DispatchPublish(publish, cancellationToken);
    }

    private async Task<ushort> DispatchPublish(Publish publish, CancellationToken cancellationToken)
    {
        if (publish.Qos != QosLevel.Qos0)
        {
            pendingPublications.TryAdd(publish.Id, publish);
        }

        await sendAction.Invoke(publish, cancellationToken);

        return publish.Id;
    }

    private void HandlePubAck(PubAck pubAck)
    {
        if (pendingPublications.TryGetValue(pubAck.Id, out var publish) && publish.Qos == QosLevel.Qos1)
        {
            pendingPublications.Remove(pubAck.Id, out _);
        }
    }

    public async Task HandlePubRecAsync(PubRec pubRec, CancellationToken cancellationToken = default)
    {
        if (!pendingPublications.TryGetValue(pubRec.Id, out var publish) && publish.Qos != QosLevel.Qos2)
        {
            return;
        }

        await sendAction.Invoke(new PubRel(pubRec.Id), cancellationToken);

        releasedPublications.Add(pubRec.Id);
    }

    private void HandlePubComp(PubComp pubComp)
    {
        if (!releasedPublications.Contains(pubComp.Id) && !pendingPublications.TryGetValue(pubComp.Id, out var publish) && publish.Qos != QosLevel.Qos2)
        {
            return;
        }

        pendingPublications.Remove(pubComp.Id, out var _);
        releasedPublications.TryTake(out _);
    }
}
