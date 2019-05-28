using HyperMsg.Mqtt.Client;
using HyperMsg.Mqtt.Serialization;
using HyperMsg.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class MqttClientTests
    {
        private readonly IPEndPoint endPoint;
        private readonly IMqttClient client;
        private readonly MqttConnectionSettings settings;
        private readonly List<Packet> receivedPackets;

        public MqttClientTests()
        {
            endPoint = new IPEndPoint(IPAddress.Loopback, 1883);
            settings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            receivedPackets = new List<Packet>();

            var builder = new ConfigurableBuilder<IMqttClient>();
            builder.UseCoreServices<Packet>(1024, 1024);
            builder.UseMqttSerializer();
            builder.UseMqttClient(settings);
            builder.UseSockets(endPoint);

            client = builder.Build();
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