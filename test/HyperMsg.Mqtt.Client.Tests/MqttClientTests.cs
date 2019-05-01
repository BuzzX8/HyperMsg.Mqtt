using FakeItEasy;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClientTests
    {
        private readonly MqttClient client;
        private readonly IConnection connection;
        private readonly ISender<Packet> sender;
        private readonly MqttConnectionSettings settings;

        private Packet sentPacket;

        public MqttClientTests()
        {
            connection = A.Fake<IConnection>();
            sender = A.Fake<ISender<Packet>>();
            settings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            client = new MqttClient(connection, sender, settings);

            A.CallTo(() => sender.SendAsync(A<Packet>._, A<CancellationToken>._))
                .Invokes(foc => sentPacket = foc.GetArgument<Packet>(0))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task ConnectAsync_Opens_Connection()
        {
            var token = default(CancellationToken);

            await client.ConnectAsync(false, token);

            A.CallTo(() => connection.OpenAsync(token)).MustHaveHappened();
        }

        [Fact]
        public async Task ConnectAsync_Sends_Correct_Packet()
        {
            await client.ConnectAsync();

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.False(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
            Assert.Equal(settings.ClientId, connPacket.ClientId);            
        }

        [Fact]
        public async Task ConnectAsync_Sends_Connect_Packet_With_CleanSession_Flag()
        {
            await client.ConnectAsync(true);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.True(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
        }
    }
}