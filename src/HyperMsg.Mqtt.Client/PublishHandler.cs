using System;
using System.Threading;
using System.Threading.Tasks;
using Qos1Dictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, System.Threading.Tasks.TaskCompletionSource<bool>>;
using Qos2Dictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, (System.Threading.Tasks.TaskCompletionSource<bool>, bool)>;
using Qos2Publish = System.Collections.Concurrent.ConcurrentDictionary<ushort, HyperMsg.Mqtt.Publish>;

namespace HyperMsg.Mqtt.Client
{
    internal class PublishHandler
    {
        private readonly IMessageSender<Packet> sender;
        private readonly Action<PublishReceivedEventArgs> receiveHandler;

        private readonly Qos1Dictionary qos1Requests;
        private readonly Qos2Dictionary qos2Requests;

        private readonly Qos2Publish qos2Receive;

        internal PublishHandler(IMessageSender<Packet> sender, Action<PublishReceivedEventArgs> receiveHandler)
        {
            qos1Requests = new Qos1Dictionary();
            qos2Requests = new Qos2Dictionary();
            qos2Receive = new Qos2Publish();
            this.receiveHandler = receiveHandler;
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

            if (request.Qos == QosLevel.Qos2)
            {
                var tsc = new TaskCompletionSource<bool>();
                qos2Requests.AddOrUpdate(publishPacket.Id, (tsc, false), (k, v) => v);
                await tsc.Task;
            }
        }

        private Publish CreatePublishPacket(PublishRequest request)
        {
            return new Publish(PacketId.New(), request.TopicName, request.Message, request.Qos);
        }

        internal void OnPubAckReceived(PubAck pubAck)
        {
            if (qos1Requests.TryRemove(pubAck.Id, out var tsc))
            {
                tsc.SetResult(true);
            }
        }

        internal void OnPubRecReceived(PubRec pubRec)
        {
            if (qos2Requests.TryGetValue(pubRec.Id, out var request))
            {
                sender.Send(new PubRel(pubRec.Id));
                var newRequest = (request.Item1, true);
                qos2Requests.TryUpdate(pubRec.Id, newRequest, request);
            }
        }

        internal void OnPubCompReceived(PubComp pubComp)
        {
            if (qos2Requests.TryGetValue(pubComp.Id, out var request) && request.Item2 && qos2Requests.TryRemove(pubComp.Id, out _))
            {
                request.Item1.SetResult(true);
            }
        }

        internal void OnPublishReceived(Publish publish)
        {
            if (publish.Qos == QosLevel.Qos0)
            {
                receiveHandler(new PublishReceivedEventArgs(publish.Topic, publish.Message));
            }

            if (publish.Qos == QosLevel.Qos1)
            {
                sender.Send(new PubAck(publish.Id));
                receiveHandler(new PublishReceivedEventArgs(publish.Topic, publish.Message));
            }

            if (publish.Qos == QosLevel.Qos2)
            {
                sender.Send(new PubRec(publish.Id));
                qos2Receive.TryAdd(publish.Id, publish);
            }
        }

        internal void OnPubRelReceived(PubRel pubRel)
        {
            if (qos2Receive.TryRemove(pubRel.Id, out var publish))
            {
                sender.Send(new PubComp(pubRel.Id));
                receiveHandler(new PublishReceivedEventArgs(publish.Topic, publish.Message));
            }
        }
    }
}
