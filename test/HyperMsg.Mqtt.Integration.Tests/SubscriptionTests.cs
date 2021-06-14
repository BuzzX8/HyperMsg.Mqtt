using HyperMsg.Mqtt;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Protocol;
using System;
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
            await StartConnectionListener();
            await ConnectAsync();

            var subscription = new SubscriptionRequest("test-topic", qosLevel);
            var subscribeTask = MessagingContext.SubscribeAsync(new[] { subscription });

            await subscribeTask.Completion;//.AsTask().Wait(DefaultWaitTimeout);
            Assert.True(subscribeTask.Completion.IsCompleted);

            Assert.Single(subscribeTask.Completion.Result);
        }

        [Fact]
        public async Task SubscribeAsync_With_MqttClient()
        {
            await StartConnectionListener();
            var client = GetService<IMqttClient>();
            var options = GetService<IMqttClientOptions>();
            await client.ConnectAsync(options);

            var subscribeOptions = new MqttClientSubscribeOptions
            {
                TopicFilters = new ()
                {
                    new MqttTopicFilter
                    {
                        QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                        Topic = Guid.NewGuid().ToString()
                    }
                }
            };
            var result = await client.SubscribeAsync(subscribeOptions, default);

            await client.DisconnectAsync();
        }
    }    
}
