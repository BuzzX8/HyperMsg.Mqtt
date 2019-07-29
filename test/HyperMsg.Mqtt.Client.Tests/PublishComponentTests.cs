using FakeItEasy;
using System.Threading;

namespace HyperMsg.Mqtt.Client
{
    public class PublishComponentTests
    {
        private readonly IMessageSender<Packet> messageSender;
        private readonly PublishComponent publishComponent;

        private readonly CancellationToken cancellationToken;

        public PublishComponentTests()
        {
            messageSender = A.Fake<IMessageSender<Packet>>();
            publishComponent = new PublishComponent(messageSender, null);
        }
    }
}
