using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class ProtocolServiceTests : ServiceHostFixture
    {
        private readonly IDataRepository dataRepository;

        public ProtocolServiceTests() : base(services => services.AddMqttServices()) =>
            dataRepository = GetRequiredService<IDataRepository>();

        #region Connection

        [Fact]
        public void Sends_Connect_Packet_When_Receives_Opening_Transport_Message()
        {
            var connectionSettings = new MqttConnectionSettings("test-client");
            dataRepository.AddOrReplace(connectionSettings);
            var sentPacket = default(Connect);

            HandlersRegistry.RegisterTransmitPipeHandler<Connect>(packet => sentPacket = packet);
            MessageSender.SendTransportMessage(TransportMessage.Opened);

            Assert.NotNull(sentPacket);
            Assert.Equal(connectionSettings.ClientId, sentPacket.ClientId);
        }

        [Fact]
        public void Sends_SetTsl_If_UseTls_Setting_True()
        {
            var connectionSettings = new MqttConnectionSettings("test-client")
            {
                UseTls = true
            };
            dataRepository.AddOrReplace(connectionSettings);
            var isMessageSend = false;

            HandlersRegistry.RegisterTransportMessageHandler(TransportMessage.SetTls, () => isMessageSend = true);
            MessageSender.SendTransportMessage(TransportMessage.Opened);

            Assert.True(isMessageSend);
        }

        [Fact]
        public void RegisterConnectionResultHandler_Invokes_Handler_For_ConnAck_Response()
        {
            var actualConAck = default(ConnAck);
            var connAck = new ConnAck(ConnectionResult.Accepted, true);

            HandlersRegistry.RegisterConnectionResultHandler(args => actualConAck = args);
            MessageSender.SendToReceivePipe(connAck);

            Assert.NotNull(actualConAck);
            Assert.Equal(connAck, actualConAck);
        }

        [Fact]
        public void RegisterConnectionResultHandler_Invokes_Async_Handler_For_ConnAck_Response()
        {
            var actualConAck = default(ConnAck);
            var connAck = new ConnAck(ConnectionResult.Accepted, true);

            HandlersRegistry.RegisterConnectionResultHandler((args, _) =>
            {
                actualConAck = args;
                return Task.CompletedTask;
            });
            MessageSender.SendToReceivePipe(connAck);

            Assert.NotNull(actualConAck);
            Assert.Equal(connAck, actualConAck);
        }

        #endregion

        #region Subscription

        [Fact]
        public async Task SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var subscribePacket = default(Subscribe);
            HandlersRegistry.RegisterTransmitPipeHandler<Subscribe>(subscribe => subscribePacket = subscribe);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();

            var packetId = await MessageSender.SendSubscriptionRequestAsync(request);

            Assert.NotNull(subscribePacket);
            Assert.Equal(packetId, subscribePacket.Id);
            Assert.True(dataRepository.Contains<Subscribe>(packetId));
        }

        [Fact]
        public void SubAck_Response_Invokes_Handler_Registered_With_RegisterSubscriptionResponseHandler()
        {
            var actualResult = default(IReadOnlyList<(string topic, SubscriptionResult result)>);
            
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();

            HandlersRegistry.RegisterSubscriptionResponseHandler(response => actualResult = response);
            var packetId = MessageSender.SendSubscriptionRequest(request);
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });
            MessageSender.SendToReceivePipe(subAck);

            Assert.NotNull(actualResult);
            Assert.False(dataRepository.Contains<Subscribe>(packetId));
        }

        [Fact]
        public async Task UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var unsubscribe = default(Unsubscribe);
            HandlersRegistry.RegisterTransmitPipeHandler<Unsubscribe>(packet => unsubscribe = packet);
            var topics = new[] { "topic-1", "topic-2" };

            var packetId = await MessageSender.SendUnsubscribeRequestAsync(topics);

            Assert.NotNull(unsubscribe);
            Assert.Equal(packetId, unsubscribe.Id);
            Assert.Equal(topics, unsubscribe.Topics);
            Assert.True(dataRepository.Contains<Unsubscribe>(unsubscribe.Id));
        }

        [Fact]
        public void UnsubAck_Response_Invokes_Handler_Registered_With_RegisterSubscriptionResponseHandler()
        {
            var actualTopics = default(IReadOnlyList<string>);
            var topics = new[] { "topic-1", "topic-2" };

            HandlersRegistry.RegisterUnsubscribeResponseHandler(response => actualTopics = response);
            var packetId = MessageSender.SendUnsubscribeRequest(topics);
            MessageSender.SendToReceivePipe(new UnsubAck(packetId));

            Assert.NotNull(actualTopics);
            Assert.False(dataRepository.Contains<Unsubscribe>(packetId));
        }

        #endregion

        [Fact]
        public void SendPublishRequest_Sends_Correct_Packet()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var qos = QosLevel.Qos1;
            var actualPacket = default(Publish);

            HandlersRegistry.RegisterTransmitPipeHandler<Publish>(publish => actualPacket = publish);
            var packetId = MessageSender.SendPublishRequest(topic, message, qos);

            Assert.NotNull(actualPacket);
            Assert.Equal(packetId, actualPacket.Id);
            Assert.Equal(topic, actualPacket.Topic);
            Assert.Equal(message, actualPacket.Message);
            Assert.Equal(qos, actualPacket.Qos);
        }

        [Fact]
        public async Task SendPublishRequestAsync_Sends_Correct_Packet()
        {
            var topic = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            var qos = QosLevel.Qos1;
            var actualPacket = default(Publish);

            HandlersRegistry.RegisterTransmitPipeHandler<Publish>(publish => actualPacket = publish);
            var packetId = await MessageSender.SendPublishRequestAsync(topic, message, qos);

            Assert.NotNull(actualPacket);
            Assert.Equal(packetId, actualPacket.Id);
            Assert.Equal(topic, actualPacket.Topic);
            Assert.Equal(message, actualPacket.Message);
            Assert.Equal(qos, actualPacket.Qos);
        }

        [Fact]
        public void SendPublishRequest_Does_Not_Stores_Publish_Packet_For_Qos0()
        {
            var packetId = MessageSender.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

            Assert.False(dataRepository.Contains<Publish>(packetId));
        }

        [Fact]
        public async Task SendPublishRequestAsync_Does_Not_Stores_Publish_Packet_For_Qos0()
        {
            var packetId = await MessageSender.SendPublishRequestAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

            Assert.False(dataRepository.Contains<Publish>(packetId));
        }

        [Fact]
        public void SendPublishRequest_Stores_Publish_Packet_For_Qos1()
        {
            var packetId = MessageSender.SendPublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

            Assert.True(dataRepository.Contains<Publish>(packetId));
        }

        [Fact]
        public async Task SendPublishRequestAsync_Stores_Publish_Packet_For_Qos1()
        {
            var packetId = await MessageSender.SendPublishRequestAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

            Assert.True(dataRepository.Contains<Publish>(packetId));
        }

        [Fact]
        public void Received_PubAck_Invokes_Handler_For_Qos1()
        {
            var actualArgs = default(PublishCompletedHandlerArgs);
            var topic = Guid.NewGuid().ToString();

            HandlersRegistry.RegisterPublishCompletedHandler(args => actualArgs = args);
            var packetId = MessageSender.SendPublishRequest(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);
            MessageSender.SendToReceivePipe(new PubAck(packetId));

            Assert.NotNull(actualArgs);
            Assert.Equal(packetId, actualArgs.Id);
            Assert.Equal(topic, actualArgs.Topic);
            Assert.Equal(QosLevel.Qos1, actualArgs.Qos);
        }
    }
}
