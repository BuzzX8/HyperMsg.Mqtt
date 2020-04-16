using FakeItEasy;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class MessageSenderExtensionsTests
    {
        private readonly IMessageSender messageSender = A.Fake<IMessageSender>();
        private readonly MqttConnectionSettings connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());

        [Fact]
        public async Task SendConnectRequestAsync_Sends_Connect_Packet()
        {
            var expectedMessage = new Connect
            {
                ClientId = connectionSettings.ClientId
            };

            await VerifySendConnectRequestAsync(expectedMessage, connectionSettings);
        }

        [Fact]
        public async Task SendConnectRequestAsync_Sends_Connect_Packet_With_CleanSession_Flag()
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
        public async Task SendConnectRequestAsync_Sends_Connect_Packet_With_KeepAlive_Specified_In_Settings()
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
        public async Task SendConnectRequestAsync_Sends_Connect_Packet_With_Corredt_WillMessageSettings()
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
            await messageSender.SendConnectRequestAsync(connectionSettings, default);

            A.CallTo(() => messageSender.SendAsync(new Transmit<Connect>(expected), A<CancellationToken>._)).MustHaveHappened();
        }
    }
}
