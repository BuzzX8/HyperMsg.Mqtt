using System;
using System.Threading;
using System.Threading.Tasks;
using Qos1Dictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, System.Threading.Tasks.TaskCompletionSource<bool>>;
using Qos2Dictionary = System.Collections.Concurrent.ConcurrentDictionary<ushort, (System.Threading.Tasks.TaskCompletionSource<bool>, bool)>;
using Qos2Publish = System.Collections.Concurrent.ConcurrentDictionary<ushort, HyperMsg.Mqtt.Publish>;

namespace HyperMsg.Mqtt.Client
{
    public class PublishComponent
    {
        private readonly IMessageSender<Packet> messageSender;
        private readonly Action<PublishReceivedEventArgs> receiveHandler;

        private readonly Qos1Dictionary qos1Requests;
        private readonly Qos2Dictionary qos2Requests;

        private readonly Qos2Publish qos2Receive;

        public PublishComponent(IMessageSender<Packet> messageSender, Action<PublishReceivedEventArgs> receiveHandler)
        {
            qos1Requests = new Qos1Dictionary();
            qos2Requests = new Qos2Dictionary();
            qos2Receive = new Qos2Publish();
            this.receiveHandler = receiveHandler;
            this.messageSender = messageSender;
        }

        public async Task SendPublishAsync(PublishRequest request, CancellationToken token)
        {
            var publishPacket = CreatePublishPacket(request);
            await messageSender.SendAsync(publishPacket, token);

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

        public void Handle(PubAck pubAck)
        {
            if (qos1Requests.TryRemove(pubAck.Id, out var tsc))
            {
                tsc.SetResult(true);
            }
        }

        public async Task HandleAsync(PubRec pubRec, CancellationToken cancellationToken)
        {
            if (qos2Requests.TryGetValue(pubRec.Id, out var request))
            {
                await messageSender.SendAsync(new PubRel(pubRec.Id), cancellationToken);
                var newRequest = (request.Item1, true);
                qos2Requests.TryUpdate(pubRec.Id, newRequest, request);
            }
        }

        public void Handle(PubComp pubComp)
        {
            if (qos2Requests.TryGetValue(pubComp.Id, out var request) && request.Item2 && qos2Requests.TryRemove(pubComp.Id, out _))
            {
                request.Item1.SetResult(true);
            }
        }

        public async Task HandleAsync(Publish publish, CancellationToken cancellationToken)
        {
            if (publish.Qos == QosLevel.Qos0)
            {
                receiveHandler(new PublishReceivedEventArgs(publish.Topic, publish.Message));
            }

            if (publish.Qos == QosLevel.Qos1)
            {
                await messageSender.SendAsync(new PubAck(publish.Id), cancellationToken);
                receiveHandler(new PublishReceivedEventArgs(publish.Topic, publish.Message));
            }

            if (publish.Qos == QosLevel.Qos2)
            {
                await messageSender.SendAsync(new PubRec(publish.Id), cancellationToken);
                qos2Receive.TryAdd(publish.Id, publish);
            }
        }

        public async Task HandleAsync(PubRel pubRel, CancellationToken cancellationToken)
        {
            if (qos2Receive.TryRemove(pubRel.Id, out var publish))
            {
                await messageSender.SendAsync(new PubComp(pubRel.Id), cancellationToken);
                receiveHandler(new PublishReceivedEventArgs(publish.Topic, publish.Message));
            }
        }
    }
}
