using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class ProtocolServiceTests : HostFixture
    {
        private readonly RequestStorage requestStorage;

        public ProtocolServiceTests() : base(services => services.AddMqttProtocolService().AddSingleton<RequestStorage>()) => 
            requestStorage = GetRequiredService<RequestStorage>();

        #region Connection

        [Fact]
        public void Sends_Connect_Packet_When_Receives_Opening_Transport_Message()
        {
            var connectionSettings = new MqttConnectionSettings("test-client");            
            var sentPacket = default(Connect);

            SenderRegistry.Register<Connect>(packet => sentPacket = packet);
            Sender.SendConnectionRequest(connectionSettings);

            Assert.NotNull(sentPacket);
            Assert.Equal(connectionSettings.ClientId, sentPacket.ClientId);
        }

        #endregion

        #region Subscription

        [Fact]
        public void SendSubscriptionRequest_Sends_Correct_Subscribe_Request()
        {
            var subscribePacket = default(Subscribe);
            SenderRegistry.Register<Subscribe>(subscribe => subscribePacket = subscribe);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();

            var packetId = Sender.SendSubscriptionRequest(request);

            Assert.NotNull(subscribePacket);
            Assert.Equal(packetId, subscribePacket.Id);
            Assert.True(requestStorage.Contains<Subscribe>(packetId));
        }

        [Fact]
        public void SubAck_Response_Invokes_Handler_Registered_With_RegisterSubscriptionResponseHandler()
        {
            var actualResult = default(SubscriptionResponseHandlerArgs);

            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();

            ReceiverRegistry.Register<SubscriptionResponseHandlerArgs>(response => actualResult = response);
            var packetId = Sender.SendSubscriptionRequest(request);
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });
            Receiver.Dispatch(subAck);

            Assert.NotNull(actualResult);
            Assert.False(requestStorage.Contains<Subscribe>(packetId));
            Assert.Equal(subAck.Results, actualResult.SubscriptionResults);
        }

        [Fact]
        public void UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var unsubscribe = default(Unsubscribe);
            SenderRegistry.Register<Unsubscribe>(packet => unsubscribe = packet);
            var topics = new[] { "topic-1", "topic-2" };

            var packetId = Sender.SendUnsubscribeRequest(topics);

            Assert.NotNull(unsubscribe);
            Assert.Equal(packetId, unsubscribe.Id);
            Assert.Equal(topics, unsubscribe.Topics);
            Assert.True(requestStorage.Contains<Unsubscribe>(unsubscribe.Id));
        }

        #endregion

        [Fact]
        public void SendPublishRequest_Sends_Correct_Packet()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var qos = QosLevel.Qos1;
            var actualPacket = default(Publish);

            SenderRegistry.Register<Publish>(publish => actualPacket = publish);
            var packetId = Sender.SendPublishRequest(topic, message, qos);

            Assert.NotNull(actualPacket);
            Assert.Equal(packetId, actualPacket.Id);
            Assert.Equal(topic, actualPacket.Topic);
            Assert.Equal(message, actualPacket.Message);
            Assert.Equal(qos, actualPacket.Qos);
        }

        [Fact]
        public void SendPublishRequestAsync_Sends_Correct_Packet()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var qos = QosLevel.Qos1;
            var actualPacket = default(Publish);

            SenderRegistry.Register<Publish>(publish => actualPacket = publish);
            var packetId = Sender.SendPublishRequest(topic, message, qos);

            Assert.NotNull(actualPacket);
            Assert.Equal(packetId, actualPacket.Id);
            Assert.Equal(topic, actualPacket.Topic);
            Assert.Equal(message, actualPacket.Message);
            Assert.Equal(qos, actualPacket.Qos);
        }

        [Fact]
        public void SendPublishRequest_Does_Not_Stores_Publish_Packet_For_Qos0()
        {
            var packetId = Sender.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

            Assert.False(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void SendPublishRequestAsync_Does_Not_Stores_Publish_Packet_For_Qos0()
        {
            var packetId = Sender.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

            Assert.False(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void SendPublishRequest_Stores_Publish_Packet_For_Qos1()
        {
            var packetId = Sender.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

            Assert.True(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void SendPublishRequestAsync_Stores_Publish_Packet_For_Qos1()
        {
            var packetId = Sender.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

            Assert.True(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void Receiving_PubAck_Invokes_Handler_For_Qos1()
        {
            var actualArgs = default(PublishCompletedHandlerArgs);
            var topic = Guid.NewGuid().ToString();

            ReceiverRegistry.Register<PublishCompletedHandlerArgs>(args => actualArgs = args);
            var packetId = Sender.SendPublishRequest(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);
            Receiver.Dispatch(new PubAck(packetId));

            Assert.NotNull(actualArgs);
            Assert.Equal(packetId, actualArgs.Id);
            Assert.Equal(topic, actualArgs.Topic);
            Assert.Equal(QosLevel.Qos1, actualArgs.Qos);
        }

        [Fact]
        public void Received_PubAck_Invokes_Async_Handler_For_Qos1()
        {
            var actualArgs = default(PublishCompletedHandlerArgs);
            var topic = Guid.NewGuid().ToString();

            ReceiverRegistry.Register<PublishCompletedHandlerArgs>(args => actualArgs = args);
            var packetId = Sender.SendPublishRequest(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);
            Receiver.Dispatch(new PubAck(packetId));

            Assert.NotNull(actualArgs);
            Assert.Equal(packetId, actualArgs.Id);
            Assert.Equal(topic, actualArgs.Topic);
            Assert.Equal(QosLevel.Qos1, actualArgs.Qos);
        }

        [Fact]
        public void Receiving_PubRec_Transmits_PubRel()
        {
            var pubRel = default(PubRel);
            var topic = Guid.NewGuid().ToString();

            SenderRegistry.Register<PubRel>(packet => pubRel = packet);
            var packetId = Sender.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos2);
            Receiver.Dispatch(new PubRec(packetId));

            Assert.NotNull(pubRel);
            Assert.Equal(packetId, pubRel.Id);
        }

        [Fact]
        public void Received_PubComp_Invokes_Handler_For_Qos2()
        {
            var actualArgs = default(PublishCompletedHandlerArgs);
            var topic = Guid.NewGuid().ToString();

            ReceiverRegistry.Register<PublishCompletedHandlerArgs>(args => actualArgs = args);
            var packetId = Sender.SendPublishRequest(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos2);
            Receiver.Dispatch(new PubRec(packetId));
            Receiver.Dispatch(new PubComp(packetId));

            Assert.NotNull(actualArgs);
            Assert.Equal(packetId, actualArgs.Id);
            Assert.Equal(topic, actualArgs.Topic);
            Assert.Equal(QosLevel.Qos2, actualArgs.Qos);
        }
    }
}
