using FakeItEasy;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectionControllerTests
    {
        private readonly ITransportCommandSender transportCommandSender;
        private readonly IMessageSender<Packet> messageSender;
        private readonly MqttConnectionSettings connectionSettings;
        private readonly ConnectionController controller;

        private readonly CancellationToken cancellationToken = new CancellationToken();
        private readonly ManualResetEventSlim packetSentEvent = new ManualResetEventSlim();
        private readonly TimeSpan waitTimeout = TimeSpan.FromSeconds(2);

        private Packet sentPacket;

        public ConnectionControllerTests()
        {
            transportCommandSender = A.Fake<ITransportCommandSender>();
            messageSender = A.Fake<IMessageSender<Packet>>();
            connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            controller = new ConnectionController(transportCommandSender, messageSender, connectionSettings);

            A.CallTo(() => transportCommandSender.SendAsync(A<TransportCommand>._, cancellationToken)).Returns(Task.CompletedTask);
            A.CallTo(() => transportCommandSender.SendAsync(A<TransportCommand>._, A<CancellationToken>._)).Invokes(foc =>
            {
                packetSentEvent.Set();
            })
            .Returns(Task.CompletedTask);

            A.CallTo(() => messageSender.SendAsync(A<Packet>._, A<CancellationToken>._)).Invokes(foc => 
            {
                    sentPacket = foc.GetArgument<Packet>(0);
                    packetSentEvent.Set();
            })
            .Returns(Task.CompletedTask);
        }

        [Fact]
        public void ConnectAsync_Sends_Open_TransportCommand()
        {
            _ = controller.ConnectAsync(false, cancellationToken);
            packetSentEvent.Wait(waitTimeout);

            A.CallTo(() => transportCommandSender.SendAsync(TransportCommand.Open, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;

            _ = controller.ConnectAsync(false, cancellationToken);
            packetSentEvent.Wait(waitTimeout);

            A.CallTo(() => transportCommandSender.SendAsync(TransportCommand.SetTransportLevelSecurity, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_Correct_Packet()
        {
            _ = controller.ConnectAsync();
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.False(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
            Assert.Equal(connectionSettings.ClientId, connPacket.ClientId);
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_CleanSession_Flag()
        {
            _ = controller.ConnectAsync(true);
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.True(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_KeepAlive_Specified_In_Settings()
        {
            connectionSettings.KeepAlive = 0x9080;
            _ = controller.ConnectAsync();
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.Equal(connectionSettings.KeepAlive, connPacket.KeepAlive);
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_Corredt_WillMessageSettings()
        {
            var willTopic = Guid.NewGuid().ToString();
            var willMessage = Guid.NewGuid().ToByteArray();
            connectionSettings.WillMessageSettings = new WillMessageSettings(willTopic, willMessage, true);

            _ = controller.ConnectAsync();
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;

            Assert.True(connPacket.Flags.HasFlag(ConnectFlags.Will));
            Assert.Equal(willTopic, connPacket.WillTopic);
            Assert.Equal(willMessage, connPacket.WillMessage);
        }

        [Fact]
        public void ConnectAsync_Returns_Correct_Result_For_Clean_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = controller.ConnectAsync();

            packetSentEvent.Wait(waitTimeout);
            controller.HandleAsync(connAck, cancellationToken);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Result);
        }

        [Fact]
        public void ConnectAsync_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = controller.ConnectAsync();

            packetSentEvent.Wait(waitTimeout);
            controller.HandleAsync(connAck, cancellationToken);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Present, task.Result);
        }

        [Fact]
        public void DisconnectAsync_Sends_Disconnect_Packet()
        {
            _ = controller.DisconnectAsync();

            packetSentEvent.Wait(waitTimeout);

            Assert.NotNull(sentPacket as Disconnect);
        }

        [Fact]
        public async Task DisconnectAsync_Submits_ClosesConnection_Command()
        {
            await controller.DisconnectAsync(cancellationToken);

            A.CallTo(() => transportCommandSender.SendAsync(TransportCommand.Close, cancellationToken)).MustHaveHappened();
        }
    }
}