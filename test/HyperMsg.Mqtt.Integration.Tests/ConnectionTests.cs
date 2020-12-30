using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class ConnectionTests
    {
        private static readonly TimeSpan waitTimeout = TimeSpan.FromSeconds(5);

        private readonly Host host;
        private readonly IMessagingContext messagingContext;
        private readonly MqttConnectionSettings connectionSettings;

        public ConnectionTests()
        {
            var services = new ServiceCollection();
            connectionSettings = new MqttConnectionSettings("HyperMsg");
            services.AddMessagingServices()
                .AddMqttServices()
                .AddSocketTransport("localhost", 1883);
            host = new Host(services);
            host.StartAsync().Wait();

            messagingContext = host.Services.GetRequiredService<IMessagingContext>();
        }

        [Fact]
        public async Task ConnectAsync_Receives_ConAck_Response()
        {            
            var conAckResponse = default(ConnAck);
            var responseEvent = new ManualResetEventSlim();
            messagingContext.Observable.OnReceived<ConnAck>(c =>
            {
                conAckResponse = c;
                responseEvent.Set();
            });

            var task = await messagingContext.ConnectAsync(connectionSettings);

            responseEvent.Wait(waitTimeout);

            Assert.True(task.IsCompleted);
            Assert.NotNull(conAckResponse);
        }

    }
}
