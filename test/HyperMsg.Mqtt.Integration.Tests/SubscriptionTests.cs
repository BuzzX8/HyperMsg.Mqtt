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
            await ConnectAsync(default);
            var subscriptionRequest = new[] { (Guid.NewGuid().ToString(), QosLevel.Qos0) };

            await await MessagingContext.StartSubscriptionAsync(subscriptionRequest, default);            

            //Assert.Single(result);
        }

        [Fact]
        public async Task Unsubscribe_Receives_UnsubAck()
        {
            await ConnectAsync(default);

            
            
            //Assert.IsType<UnsubAck>(LastResponse);
        }
    }
}