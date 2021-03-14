using HyperMsg.Mqtt.Extensions;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class SubscriptionTests : IntegrationTestBase
    {
        [Theory]
        [InlineData(QosLevel.Qos0)]
        [InlineData(QosLevel.Qos1)]
        [InlineData(QosLevel.Qos2)]
        public async Task SubscribeAsync_Receives_Subscription_Result(QosLevel qosLevel)
        {
            await ConnectAsync();

            var subscription = new SubscriptionRequest("test-topic", qosLevel);
            var subscribeTask = await MessagingContext.SubscribeAsync(new[] { subscription });

            subscribeTask.AsTask().Wait(DefaultWaitTimeout);
            Assert.True(subscribeTask.IsCompleted);

            Assert.Single(subscribeTask.Result);
        }
    }    
}
