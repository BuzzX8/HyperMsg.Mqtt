﻿using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
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
        public void ConnAck_Response_Invokes_Handler_Registered_With_RegisterSubscriptionResponseHandler()
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
    }
}
