using HyperMsg.Mqtt.Packets;
using System;
using System.Linq;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class ProtocolServiceTests
    {
        private readonly MessageBroker messageBroker;
        private readonly RequestStorage requestStorage;
        private readonly ProtocolService service;

        public ProtocolServiceTests()
        {
            messageBroker = new();
            requestStorage = new();
            service = new(messageBroker, messageBroker, requestStorage);
        }

        #region Connection

        [Fact]
        public void Sends_Connect_Packet_When_Receives_Opening_Transport_Message()
        {
            var connectionSettings = new MqttConnectionSettings("test-client");            
            var sentPacket = default(Connect);

            messageBroker.Register<Connect>(packet => sentPacket = packet);
            messageBroker.SendConnectionRequest(connectionSettings);

            Assert.NotNull(sentPacket);
            Assert.Equal(connectionSettings.ClientId, sentPacket.ClientId);
        }

        #endregion

        #region Subscription

        [Fact]
        public void SendSubscriptionRequest_Sends_Correct_Subscribe_Request()
        {
            var subscribePacket = default(Subscribe);
            messageBroker.Register<Subscribe>(subscribe => subscribePacket = subscribe);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();

            var packetId = messageBroker.SendSubscriptionRequest(request);

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

            messageBroker.Register<SubscriptionResponseHandlerArgs>(response => actualResult = response);
            var packetId = messageBroker.SendSubscriptionRequest(request);
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });
            messageBroker.Dispatch(subAck);

            Assert.NotNull(actualResult);
            Assert.False(requestStorage.Contains<Subscribe>(packetId));
            Assert.Equal(subAck.Results, actualResult.SubscriptionResults);
        }

        [Fact]
        public void UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var unsubscribe = default(Unsubscribe);
            messageBroker.Register<Unsubscribe>(packet => unsubscribe = packet);
            var topics = new[] { "topic-1", "topic-2" };

            var packetId = messageBroker.SendUnsubscribeRequest(topics);

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

            messageBroker.Register<Publish>(publish => actualPacket = publish);
            var packetId = messageBroker.SendPublishRequest(topic, message, qos);

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

            messageBroker.Register<Publish>(publish => actualPacket = publish);
            var packetId = messageBroker.SendPublishRequest(topic, message, qos);

            Assert.NotNull(actualPacket);
            Assert.Equal(packetId, actualPacket.Id);
            Assert.Equal(topic, actualPacket.Topic);
            Assert.Equal(message, actualPacket.Message);
            Assert.Equal(qos, actualPacket.Qos);
        }

        [Fact]
        public void SendPublishRequest_Does_Not_Stores_Publish_Packet_For_Qos0()
        {
            var packetId = messageBroker.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

            Assert.False(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void SendPublishRequestAsync_Does_Not_Stores_Publish_Packet_For_Qos0()
        {
            var packetId = messageBroker.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

            Assert.False(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void SendPublishRequest_Stores_Publish_Packet_For_Qos1()
        {
            var packetId = messageBroker.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

            Assert.True(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void SendPublishRequestAsync_Stores_Publish_Packet_For_Qos1()
        {
            var packetId = messageBroker.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

            Assert.True(requestStorage.Contains<Publish>(packetId));
        }

        [Fact]
        public void Receiving_PubAck_Invokes_Handler_For_Qos1()
        {
            var actualArgs = default(PublishCompletedHandlerArgs);
            var topic = Guid.NewGuid().ToString();

            messageBroker.Register<PublishCompletedHandlerArgs>(args => actualArgs = args);
            var packetId = messageBroker.SendPublishRequest(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);
            messageBroker.Dispatch(new PubAck(packetId));

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

            messageBroker.Register<PublishCompletedHandlerArgs>(args => actualArgs = args);
            var packetId = messageBroker.SendPublishRequest(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);
            messageBroker.Dispatch(new PubAck(packetId));

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

            messageBroker.Register<PubRel>(packet => pubRel = packet);
            var packetId = messageBroker.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos2);
            messageBroker.Dispatch(new PubRec(packetId));

            Assert.NotNull(pubRel);
            Assert.Equal(packetId, pubRel.Id);
        }

        [Fact]
        public void Received_PubComp_Invokes_Handler_For_Qos2()
        {
            var actualArgs = default(PublishCompletedHandlerArgs);
            var topic = Guid.NewGuid().ToString();

            messageBroker.Register<PublishCompletedHandlerArgs>(args => actualArgs = args);
            var packetId = messageBroker.SendPublishRequest(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos2);
            messageBroker.Dispatch(new PubRec(packetId));
            messageBroker.Dispatch(new PubComp(packetId));

            Assert.NotNull(actualArgs);
            Assert.Equal(packetId, actualArgs.Id);
            Assert.Equal(topic, actualArgs.Topic);
            Assert.Equal(QosLevel.Qos2, actualArgs.Qos);
        }
    }
}
