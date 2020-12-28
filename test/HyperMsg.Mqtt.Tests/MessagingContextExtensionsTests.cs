using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Serialization
{
    public class MessagingContextExtensionsTests
    {
        private readonly Host host;
        private readonly MqttConnectionSettings connectionSettings;
        private readonly IMessagingContext messagingContext;
        private readonly IMessageObservable observable;

        private readonly CancellationTokenSource tokenSource;

        public MessagingContextExtensionsTests()
        {
            var services = new ServiceCollection();
            connectionSettings = new MqttConnectionSettings("test-client");
            services.AddMessagingServices();
            services.AddMqttServices(connectionSettings);
            host = new Host(services);
            host.StartAsync().Wait();

            messagingContext = host.Services.GetRequiredService<IMessagingContext>();
            observable = host.Services.GetRequiredService<IMessageObservable>();
            tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public async Task ConnectAsync_Sends_Open_TransportCommand()
        {
            var command = default(TransportCommand);
            observable.Subscribe<TransportCommand>(c => command = c);
            await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.Equal(TransportCommand.Open, command);
        }

        [Fact]
        public async Task ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;
            var command = default(TransportCommand);
            observable.Subscribe<TransportCommand>(c => command = c);
            await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

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

        private async Task VerifyTransmittedConnectPacket(Connect expectedPacket)
        {
            var actualPacket = default(Connect);
            observable.OnTransmit<Connect>(c => actualPacket = c);
            await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public async Task Received_Connack_Completes_Connect_Task_With_Correct_Result_For_SessionState()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            messagingContext.Sender.Received(connAck);
                        
            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Result);
        }

        [Fact]
        public async Task Received_ConAck_Completes_Connect_Task_And_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            messagingContext.Sender.Received(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Present, task.Result);
        }
    }
}
