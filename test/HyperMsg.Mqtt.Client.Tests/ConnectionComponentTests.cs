using FakeItEasy;
using System;
using System.Threading;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectionComponentTests
    {
        private readonly AsyncAction<TransportCommand> transportCommandHandler;
        private readonly IMessageSender<Packet> messageSender;
        private readonly MqttConnectionSettings connectionSettings;
        private readonly ConnectionComponent connectionComponent;

        private readonly CancellationToken cancellationToken;

        public ConnectionComponentTests()
        {
            transportCommandHandler = A.Fake<AsyncAction<TransportCommand>>();
            messageSender = A.Fake<IMessageSender<Packet>>();
            connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            connectionComponent = new ConnectionComponent(transportCommandHandler, messageSender, connectionSettings);
        }

        [Fact]
        public void ConnectAsync_Sends_Open_TransportCommand()
        {
            _ = connectionComponent.ConnectAsync(false, cancellationToken);

            A.CallTo(() => transportCommandHandler.Invoke(TransportCommand.Open, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;

            _ = connectionComponent.ConnectAsync(false, cancellationToken);

            A.CallTo(() => transportCommandHandler.Invoke(TransportCommand.SetTransportLevelSecurity, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_Correct_Packet()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId
            };

            _ = connectionComponent.ConnectAsync(false, cancellationToken);

            A.CallTo(() => messageSender.SendAsync(expectedPacket, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_CleanSession_Flag()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId,
                Flags = ConnectFlags.CleanSession
            };

            _ = connectionComponent.ConnectAsync(true, cancellationToken);

            A.CallTo(() => messageSender.SendAsync(expectedPacket, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_KeepAlive_Specified_In_Settings()
        {
            connectionSettings.KeepAlive = 0x9080;
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId,
                KeepAlive = connectionSettings.KeepAlive
            };

            _ = connectionComponent.ConnectAsync();

            A.CallTo(() => messageSender.SendAsync(expectedPacket, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_Corredt_WillMessageSettings()
        {
            var willTopic = Guid.NewGuid().ToString();
            var willMessage = Guid.NewGuid().ToByteArray();
            connectionSettings.WillMessageSettings = new WillMessageSettings(willTopic, willMessage, true);
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId,
                Flags = ConnectFlags.Will,
                WillTopic = willTopic,
                WillMessage = willMessage
            };

            _ = connectionComponent.ConnectAsync();

            A.CallTo(() => messageSender.SendAsync(expectedPacket, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void Handle_Completes_Connect_Task_With_Correct_Result_For_SessionState()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = connectionComponent.ConnectAsync();
                        
            connectionComponent.Handle(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Result);
        }

        [Fact]
        public void Handle_Completes_Connect_Task_And_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = connectionComponent.ConnectAsync();
                        
            connectionComponent.Handle(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Present, task.Result);
        }
    }
}
