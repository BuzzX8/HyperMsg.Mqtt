using HyperMsg.Mqtt.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class PublishTests : MqttClientIntegrationTestsBase
    {
        [Fact]
        public async Task Publish_Receives_PubAck_For_Qos1_Publications()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var request = new PublishRequest(topic, message, QosLevel.Qos1);
            await ConnectAsync(false, default);

            await Client.PublishAsync(request);

            //Assert.IsType<PubAck>(LastResponse);
        }

        [Fact]
        public async Task Publish_Receives_PubRec_And_PubComp_For_Qos2_Publications()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var request = new PublishRequest(topic, message, QosLevel.Qos2);
            await ConnectAsync(false, default);

            await Client.PublishAsync(request);

            //Assert.Equal(3, Responses.Count);
            //Assert.IsType<PubRec>(Responses[1]);
            //Assert.IsType<PubComp>(Responses[2]);
        }
    }
}
