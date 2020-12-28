using FakeItEasy;
using HyperMsg.Mqtt.Packets;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class MessageSenderExtensionsTests
    {
        private readonly IMessageSender messageSender = A.Fake<IMessageSender>();
        private readonly MqttConnectionSettings connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());

        [Fact]
        public async Task TransmitConnectAsync_Transmits_Connect_Packet()
        {
            var expectedMessage = new Connect
            {
                ClientId = connectionSettings.ClientId
            };

            await VerifySendConnectRequestAsync(expectedMessage, connectionSettings);
        }

        [Fact]
        public async Task TransmitConnectAsync_Transmits_Connect_Packet_With_CleanSession_Flag()
        {
            var expectedMessage = new Connect
            {
                ClientId = connectionSettings.ClientId,
                Flags = ConnectFlags.CleanSession
            };
            connectionSettings.CleanSession = true;

            await VerifySendConnectRequestAsync(expectedMessage, connectionSettings);
        }

        [Fact]
        public async Task TransmitConnectAsync_Transmits_Connect_Packet_With_KeepAlive_Specified_In_Settings()
        {
            connectionSettings.KeepAlive = 0x9080;
            var expectedMessage = new Connect
            {
                ClientId = connectionSettings.ClientId,
                KeepAlive = connectionSettings.KeepAlive
            };

            await VerifySendConnectRequestAsync(expectedMessage, connectionSettings);
        }

        [Fact]
        public async Task TransmitConnectAsync_Transmits_Connect_Packet_With_Corredt_WillMessageSettings()
        {
            var willTopic = Guid.NewGuid().ToString();
            var willMessage = Guid.NewGuid().ToByteArray();
            connectionSettings.WillMessageSettings = new WillMessageSettings(willTopic, willMessage, true);
            var expectedMessage = new Connect
            {
                ClientId = connectionSettings.ClientId,
                Flags = ConnectFlags.Will,
                WillTopic = willTopic,
                WillMessage = willMessage
            };

            await VerifySendConnectRequestAsync(expectedMessage, connectionSettings);
        }

        private async Task VerifySendConnectRequestAsync(Connect expected, MqttConnectionSettings connectionSettings)
        {
            await messageSender.TransmitConnectAsync(connectionSettings, default);

            A.CallTo(() => messageSender.SendAsync(new Transmit<Connect>(expected), A<CancellationToken>._)).MustHaveHappened();
        }

        [Fact]
        public async Task TransmitDisconnectAsync_Transmits_Correct_Packet()
        {
            await messageSender.TransmitDisconnectAsync(default);

            A.CallTo(() => messageSender.SendAsync(new Transmit<Disconnect>(Disconnect.Instance), default)).MustHaveHappened();
        }

        [Fact]
        public async Task TransmitSubscribeAsync_Transmits_Correct_Subscribe_Request()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => ($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var packetId = (ushort)0x8888;
            var expectedMessage = new Subscribe(packetId, request);

            await messageSender.TransmitSubscribeAsync(packetId, request, default);

            A.CallTo(() => messageSender.SendAsync(new Transmit<Subscribe>(expectedMessage), default)).MustHaveHappened();
        }

        [Fact]
        public async Task TransmitUnsubscribeAsync_Transmits_Correct_Unsubscribe_Request()
        {
            var topics = new[] { "topic-1", "topic-2" };
            var packetId = (ushort)0x8888;
            var expectedMessage = new Unsubscribe(packetId, topics);

            await messageSender.TransmitUnsubscribeAsync(packetId, topics, default);

            A.CallTo(() => messageSender.SendAsync(new Transmit<Unsubscribe>(expectedMessage), default)).MustHaveHappened();
        }

        [Fact]
        public async Task TransmitPublishAsync_Transmits_Correct_Publish_Packet()
        {
            var expectedMessage = new Publish(0x8080, Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

            await messageSender.TransmitPublishAsync(expectedMessage.Id, expectedMessage.Topic, expectedMessage.Qos, expectedMessage.Message, false, false, default);

            A.CallTo(() => messageSender.SendAsync(new Transmit<Publish>(expectedMessage), default)).MustHaveHappened();
        }

        [Fact]
        public async Task TransmitPingAsync_Transmits_Ping_Packet()
        {
            await messageSender.TransmitPingReqAsync(default);

            A.CallTo(() => messageSender.SendAsync(new Transmit<PingReq>(PingReq.Instance), default)).MustHaveHappened();
        }
    }
}