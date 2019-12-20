using System.Linq;
using System.Threading;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class SubscriptionComponentTests
    {
        private readonly FakeMessageSender messageSender;
        private readonly SubscriptionComponent subscriptionComponent;
        private readonly CancellationTokenSource tokenSource;

        public SubscriptionComponentTests()
        {
            messageSender = new FakeMessageSender();
            subscriptionComponent = new SubscriptionComponent(messageSender);
            tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public void SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            _ = subscriptionComponent.SubscribeAsync(request, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var subscribePacket = messageSender.GetLastTransmit<Subscribe>();
            Assert.NotNull(subscribePacket);
        }

        [Fact]
        public void SubscribeAsync_Returns_SubscriptionResult_When_SubAck_Received()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var task = subscriptionComponent.SubscribeAsync(request, tokenSource.Token);
            messageSender.WaitMessageToSent();
            var packetId = messageSender.GetLastTransmit<Subscribe>().Id;
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });

            subscriptionComponent.Handle(subAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(subAck.Results, task.Result);
        }

        [Fact]
        public void UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var topics = new[] { "topic-1", "topic-2" };

            _ = subscriptionComponent.UnsubscribeAsync(topics, tokenSource.Token);

            var unsubscribe = messageSender.GetLastTransmit<Unsubscribe>();

            Assert.NotNull(unsubscribe);
            Assert.Equal(topics, unsubscribe.Topics);
        }

        [Fact]
        public void UnsubscribeAsync_Completes_Task_When_UnsubAck_Received()
        {
            var topics = new[] { "topic-1", "topic-2" };
            var task = subscriptionComponent.UnsubscribeAsync(topics, tokenSource.Token);            
            var unsubscribe = messageSender.GetLastTransmit<Unsubscribe>();

            subscriptionComponent.Handle(new UnsubAck(unsubscribe.Id));

            Assert.True(task.IsCompleted);
        }
    }
}
