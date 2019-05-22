using HyperMsg.Mqtt.Client;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class MqttClientTests
    {
        private readonly IMqttClient client;
        private readonly List<Packet> receivedPackets;

        public MqttClientTests()
        {
            receivedPackets = new List<Packet>();
        }
        
        [Fact]
        public void Connect_Receives_ConnAck_From_Server()
        {
            client.Connect(true);

            var response = receivedPackets.SingleOrDefault() as ConnAck;

            Assert.NotNull(response);
        }
    }
}