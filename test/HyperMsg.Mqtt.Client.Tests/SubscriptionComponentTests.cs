using FakeItEasy;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class SubscriptionComponentTests
    {
        private readonly IMessageSender<Packet> messageSender;
        private readonly SubscriptionComponent subscriptionComponent;
        private readonly CancellationToken cancellationToken;

        private Packet sentPacket;

        public SubscriptionComponentTests()
        {
            messageSender = A.Fake<IMessageSender<Packet>>();
            subscriptionComponent = new SubscriptionComponent(messageSender);
            cancellationToken = new CancellationToken();

            A.CallTo(() => messageSender.SendAsync(A<Packet>._, A<CancellationToken>._))
                .Invokes(foc =>
                {
                    sentPacket = foc.GetArgument<Packet>(0);
                })
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public void SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            _ = subscriptionComponent.SubscribeAsync(request, cancellationToken);

            var subscribePacket = sentPacket as Subscribe;
            Assert.NotNull(subscribePacket);
        }

        [Fact]
        public void SubscribeAsync_Returns_SubscriptionResult_When_SubAck_Received()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var task = subscriptionComponent.SubscribeAsync(request, cancellationToken);
            var packetId = ((Subscribe)sentPacket).Id;
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });

            subscriptionComponent.Handle(subAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(subAck.Results, task.Result);
        }

        [Fact]
        public void UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var topics = new[] { "topic-1", "topic-2" };

            subscriptionComponent.UnsubscribeAsync(topics, cancellationToken);
            //packetSentEvent.Wait(waitTimeout);

            var unsubscribe = sentPacket as Unsubscribe;

            Assert.NotNull(unsubscribe);
            Assert.Equal(topics, unsubscribe.Topics);
        }

        [Fact]
        public async Task UnsubscribeAsync_Completes_Task_When_UnsubAck_Received()
        {
            var topics = new[] { "topic-1", "topic-2" };
            var task = subscriptionComponent.UnsubscribeAsync(topics, cancellationToken);
            //packetSentEvent.Wait(waitTimeout);
            var unsubscribe = sentPacket as Unsubscribe;

            subscriptionComponent.Handle(new UnsubAck(unsubscribe.Id));

            Assert.True(task.IsCompleted);
        }
    }
}
