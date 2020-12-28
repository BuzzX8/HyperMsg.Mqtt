using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class ConnectionComponentTests
    {
        private readonly Host host;
        private readonly ConnectionComponent connectionComponent;
        private readonly MqttConnectionSettings connectionSettings;
        private readonly IMessageObservable observable;

        private readonly CancellationTokenSource tokenSource;

        public ConnectionComponentTests()
        {
            var services = new ServiceCollection();
            connectionSettings = new MqttConnectionSettings("test-client");
            services.AddMessagingServices();
            services.AddMqttServices(connectionSettings);
            host = new Host(services);
            host.StartAsync().Wait();

            connectionComponent = host.Services.GetRequiredService<ConnectionComponent>();
            observable = host.Services.GetRequiredService<IMessageObservable>();
            
            tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public async Task ConnectAsync_Sends_Open_TransportCommand()
        {
            var command = default(TransportCommand);
            observable.Subscribe<TransportCommand>(c => command = c);
            await connectionComponent.ConnectAsync(false, tokenSource.Token);
                        
            Assert.Equal(TransportCommand.Open, command);
        }

        [Fact]
        public async Task ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;
            var command = default(TransportCommand);
            observable.Subscribe<TransportCommand>(c => command = c);
            await connectionComponent.ConnectAsync(false, tokenSource.Token);            
                        
            Assert.Equal(TransportCommand.SetTransportLevelSecurity, command);
        }

        [Fact]
        public async Task ConnectAsync_Sends_Correct_Packet()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId
            };

            await VerifyTransmittedConnectPacket(expectedPacket);
        }

        [Fact]
        public async Task ConnectAsync_Sends_Connect_Packet_With_CleanSession_Flag()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId,
                Flags = ConnectFlags.CleanSession
            };

            await VerifyTransmittedConnectPacket(expectedPacket, true);
        }

        

        [Fact]
        public async Task ConnectAsync_Sends_Connect_Packet_With_KeepAlive_Specified_In_Settings()
        {
            connectionSettings.KeepAlive = 0x9080;
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId,
                KeepAlive = connectionSettings.KeepAlive
            };

            await VerifyTransmittedConnectPacket(expectedPacket);
        }        

        [Fact]
        public async Task ConnectAsync_Sends_Connect_Packet_With_Corredt_WillMessageSettings()
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

            await VerifyTransmittedConnectPacket(expectedPacket);
        }

        private async Task VerifyTransmittedConnectPacket(Connect expectedPacket, bool cleanSession = false)
        {
            var actualPacket = default(Connect);
            observable.OnTransmit<Connect>(c => actualPacket = c);
            await connectionComponent.ConnectAsync(cleanSession, tokenSource.Token);

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public async Task Handle_Completes_Connect_Task_With_Correct_Result_For_SessionState()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = await connectionComponent.ConnectAsync();

            connectionComponent.Handle(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Result);
        }

        [Fact]
        public async Task Handle_Completes_Connect_Task_And_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = await connectionComponent.ConnectAsync();

            connectionComponent.Handle(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Present, task.Result);
        }
    }
}
