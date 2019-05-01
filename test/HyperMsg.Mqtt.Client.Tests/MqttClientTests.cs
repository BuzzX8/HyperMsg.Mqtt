using FakeItEasy;
using System;
using System.Linq;
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
        private readonly TimeSpan waitTimeout;

        private ManualResetEventSlim packetSentEvent;
        private Packet sentPacket;

        public MqttClientTests()
        {
            connection = A.Fake<IConnection>();
            sender = A.Fake<ISender<Packet>>();
            settings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            client = new MqttClient(connection, sender, settings);

            packetSentEvent = new ManualResetEventSlim();
            A.CallTo(() => sender.SendAsync(A<Packet>._, A<CancellationToken>._))
                .Invokes(foc =>
                {
                    sentPacket = foc.GetArgument<Packet>(0);
                    packetSentEvent.Set();
                })
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public void ConnectAsync_Opens_Connection()
        {
            var token = default(CancellationToken);

            _ = client.ConnectAsync(false, token);
            packetSentEvent.Wait(waitTimeout);

            A.CallTo(() => connection.OpenAsync(token)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_Correct_Packet()
        {
            _ = client.ConnectAsync();
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.False(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
            Assert.Equal(settings.ClientId, connPacket.ClientId);            
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_CleanSession_Flag()
        {
            _ = client.ConnectAsync(true);
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.True(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
        }

        [Fact]
        public void ConnectAsync_Returns_Correct_Result_For_Clean_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = client.ConnectAsync();

            packetSentEvent.Wait(waitTimeout);
            client.OnPacketReceived(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Result);
        }

        [Fact]
        public void ConnectAsync_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = client.ConnectAsync();

            packetSentEvent.Wait(waitTimeout);
            client.OnPacketReceived(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Present, task.Result);
        }

        [Fact]
        public void DisconnectAsync_Sends_Disconnect_Packet()
        {
            _ = client.DisconnectAsync();

            packetSentEvent.Wait(waitTimeout);

            Assert.NotNull(sentPacket as Disconnect);
        }

        [Fact]
        public async Task DisconnectAsync_Closes_Connection()
        {
            var token = default(CancellationToken);

            await client.DisconnectAsync(token);

            A.CallTo(() => connection.CloseAsync(token)).MustHaveHappened();
        }

        [Fact]
        public void SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            _ = client.SubscribeAsync(request);
            packetSentEvent.Wait(waitTimeout);

            var subscribePacket = sentPacket as Subscribe;
            Assert.NotNull(subscribePacket);
        }

        [Fact]
        public void SubscribeAsync_Returns_SubscriptionResult_When_SubAck_Received()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var task = client.SubscribeAsync(request);
            packetSentEvent.Wait(waitTimeout);
            var packetId = ((Subscribe)sentPacket).Id;
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });

            client.OnPacketReceived(subAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(subAck.Results, task.Result);
        }
    }
}