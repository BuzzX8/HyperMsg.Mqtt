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

        private static readonly TimeSpan waitTimeout = TimeSpan.FromSeconds(2);

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
                var repository = (IHandlerRegistry)p.GetService(typeof(IHandlerRegistry));
                var messageInterceptor = new DelegateHandler<Packet>(packet =>
                {
                    receivedPackets.Add(packet);
                });
                //repository.Register(messageInterceptor);
            });

            client = builder.Build();
        }
        
        [Fact]
        public void Connect_Receives_ConnAck_From_Server()
        {
            client.ConnectAsync(true).Wait(waitTimeout);

            var response = receivedPackets.SingleOrDefault() as ConnAck;

            Assert.NotNull(response);
        }

        [Fact]
        public void Ping_Receives_PingResp()
        {
            Connect();

            client.PingAsync(default).Wait(waitTimeout);
            var response = receivedPackets.Last() as PingResp;

            Assert.NotNull(response);
        }

        [Fact]
        public void Subscribe_Receives_SubAck()
        {
            var subscriptionRequest = new SubscriptionRequest(Guid.NewGuid().ToString(), QosLevel.Qos0);
            Connect();

            client.SubscribeAsync(new[] { subscriptionRequest }).Wait(waitTimeout);
            var response = receivedPackets.Last() as SubAck;

            Assert.NotNull(response);
        }

        [Fact]
        public void Unsubscribe_Receives_UnsubAck()
        {
            Connect();

            client.UnsubscribeAsync(new[] { Guid.NewGuid().ToString() }).Wait(waitTimeout);
            var response = receivedPackets.Last() as UnsubAck;

            Assert.NotNull(response);
        }

        [Fact]
        public void Publish_Receives_PubAck_For_Qos1_Publications()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var request = new PublishRequest(topic, message, QosLevel.Qos1);
            Connect();

            client.PublishAsync(request).Wait(waitTimeout);
            var response = receivedPackets.Last() as PubAck;

            Assert.NotNull(response);
        }

        [Fact]
        public void Publish_Receives_PubRec_And_PubComp_For_Qos2_Publications()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var request = new PublishRequest(topic, message, QosLevel.Qos2);
            Connect();

            client.PublishAsync(request).Wait();

            Assert.Equal(3, receivedPackets.Count);
            Assert.IsType<PubRec>(receivedPackets[1]);
            Assert.IsType<PubComp>(receivedPackets[2]);
        }

        private void Connect() => client.ConnectAsync(true).Wait();

        public void Dispose() => client.DisconnectAsync().Wait(waitTimeout);
    }
}