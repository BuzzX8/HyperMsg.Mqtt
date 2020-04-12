using HyperMsg.Transport;
using System;
using System.Threading;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectionComponentTests
    {        
        private readonly FakeMessageSender messageSender;
        private readonly MqttConnectionSettings connectionSettings;
        private readonly ConnectionComponent connectionComponent;

        private readonly CancellationTokenSource tokenSource;

        public ConnectionComponentTests()
        {            
            messageSender = new FakeMessageSender();
            connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            connectionComponent = new ConnectionComponent(messageSender, connectionSettings);
            tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public void ConnectAsync_Sends_Open_TransportCommand()
        {
            _ = connectionComponent.ConnectAsync(false, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var actual = messageSender.GetFirstMessage<TransportCommand>();
            Assert.Equal(TransportCommand.Open, actual);
        }

        [Fact]
        public void ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;

            _ = connectionComponent.ConnectAsync(false, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var actual = messageSender.GetFirstMessage<TransportCommand>();
            Assert.Equal(TransportCommand.Open, actual);
        }

        [Fact]
        public void ConnectAsync_Sends_Correct_Packet()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId
            };

            _ = connectionComponent.ConnectAsync(false, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var actual = messageSender.GetLastTransmit<Connect>();
            Assert.Equal(expectedPacket, actual);
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_CleanSession_Flag()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId,
                Flags = ConnectFlags.CleanSession
            };

            _ = connectionComponent.ConnectAsync(true, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var actual = messageSender.GetLastTransmit<Connect>();
            Assert.Equal(expectedPacket, actual);
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

            var actual = messageSender.GetLastTransmit<Connect>();
            Assert.Equal(expectedPacket, actual);
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
            messageSender.WaitMessageToSent();

            var actual = messageSender.GetLastTransmit<Connect>();
            Assert.Equal(expectedPacket, actual);
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
