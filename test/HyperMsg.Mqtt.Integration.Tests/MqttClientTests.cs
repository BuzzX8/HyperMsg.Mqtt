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
    public class MqttClientTests : IDisposable
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
            builder.RegisterConfigurator((p, s) =>
            {
                var repository = (IHandlerRepository)p.GetService(typeof(IHandlerRepository));
                var messageInterceptor = new DelegateHandler<Packet>(packet =>
                {
                    receivedPackets.Add(packet);
                });
                repository.AddHandler(messageInterceptor);
            });

            client = builder.Build();
        }
        
        [Fact]
        public void Connect_Receives_ConnAck_From_Server()
        {
            client.Connect(true);

            var response = receivedPackets.SingleOrDefault() as ConnAck;

            Assert.NotNull(response);
        }

        [Fact]
        public void Ping_Receives_PingResp()
        {
            client.Connect(true);

            client.Ping();
            var response = receivedPackets.Last() as PingResp;

            Assert.NotNull(response);
        }

        [Fact]
        public void Subscribe_Receives_SubAck()
        {
            var subscriptionRequest = new SubscriptionRequest(Guid.NewGuid().ToString(), QosLevel.Qos0);
            client.Connect(true);

            client.Subscribe(new[] { subscriptionRequest });
            var response = receivedPackets.Last() as SubAck;

            Assert.NotNull(response);
        }

        [Fact]
        public void Unsubscribe_Receives_UnsubAck()
        {
            client.Connect(true);

            client.Unsubscribe(new[] { Guid.NewGuid().ToString() });
            var response = receivedPackets.Last() as UnsubAck;

            Assert.NotNull(response);
        }

        [Fact]
        public void Publish_Receives_PubAck_For_Qos1_Publications()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            client.Connect(true);

            client.Publish(new PublishRequest(topic, message, QosLevel.Qos1));
            var response = receivedPackets.Last() as PubAck;

            Assert.NotNull(response);
        }

        public void Dispose()
        {
            client.Disconnect();
        }
    }
}