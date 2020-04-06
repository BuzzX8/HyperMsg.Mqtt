using HyperMsg.Mqtt.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class SubscriptionTests : MqttClientIntegrationTestsBase
    {
        [Fact]
        public async Task Subscribe_Receives_SubAck()
        {
            var subscriptionRequest = new SubscriptionRequest(Guid.NewGuid().ToString(), QosLevel.Qos0);
            await ConnectAsync(false, default);

            var result = await Client.SubscribeAsync(new[] { subscriptionRequest });

            Assert.Single(result);
        }

        [Fact]
        public async Task Unsubscribe_Receives_UnsubAck()
        {
            await ConnectAsync(false, default);

            await Client.UnsubscribeAsync(new[] { Guid.NewGuid().ToString() });
            
            //Assert.IsType<UnsubAck>(LastResponse);
        }
    }
}