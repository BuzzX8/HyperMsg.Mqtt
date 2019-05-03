using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    internal class PublishHandler
    {
        private readonly ISender<Packet> sender;

        internal PublishHandler(ISender<Packet> sender)
        {
            this.sender = sender;
        }

        internal async Task SendPublishAsync(PublishRequest request, CancellationToken token)
        {
            var publishPacket = CreatePublishPacket(request);
            await sender.SendAsync(publishPacket, token);
        }

        private Publish CreatePublishPacket(PublishRequest request)
        {
            return new Publish(PacketId.New(), request.Message.ToArray()) { Topic = request.TopicName };
        }

        internal void OnPubAckReceived(PubAck pubAck)
        { }

        internal void OnPubRecReceived(PubRec pubRec)
        { }

        internal void OnPubCompReceived(PubComp pubComp)
        { }
    }
}
