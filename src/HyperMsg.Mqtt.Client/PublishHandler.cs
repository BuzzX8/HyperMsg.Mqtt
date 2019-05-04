using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    internal class PublishHandler
    {
        private readonly ISender<Packet> sender;

        private readonly ConcurrentDictionary<ushort, TaskCompletionSource<bool>> qos1Requests;

        internal PublishHandler(ISender<Packet> sender)
        {
            qos1Requests = new ConcurrentDictionary<ushort, TaskCompletionSource<bool>>();
            this.sender = sender;
        }

        internal async Task SendPublishAsync(PublishRequest request, CancellationToken token)
        {
            var publishPacket = CreatePublishPacket(request);
            await sender.SendAsync(publishPacket, token);

            if(request.Qos == QosLevel.Qos1)
            {
                var tsc = new TaskCompletionSource<bool>();
                qos1Requests.AddOrUpdate(publishPacket.Id, tsc, (k, v) => v);
                await tsc.Task;
            }
        }

        private Publish CreatePublishPacket(PublishRequest request)
        {
            return new Publish(PacketId.New(), request.Message.ToArray()) { Topic = request.TopicName };
        }

        internal void OnPubAckReceived(PubAck pubAck)
        {
            if (qos1Requests.TryRemove(pubAck.Id, out var tsc))
            {
                tsc.SetResult(true);
            }
        }

        internal void OnPubRecReceived(PubRec pubRec)
        { }

        internal void OnPubCompReceived(PubComp pubComp)
        { }
    }
}
