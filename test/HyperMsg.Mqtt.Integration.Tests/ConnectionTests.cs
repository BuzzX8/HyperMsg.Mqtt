using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class ConnectionTests : IntegrationTestBase
    {
        [Fact]
        public async Task ConnectAsync_Receives_ConAck_Response()
        {            
            var conAckResponse = default(ConnAck);
            MessageObservable.OnReceived<ConnAck>(c => conAckResponse = c);

            var task = await MessagingContext.ConnectAsync(ConnectionSettings);

            task.AsTask().Wait(DefaultWaitTimeout);

            Assert.True(task.IsCompleted);
            Assert.NotNull(conAckResponse);
        }

        [Fact]
        public async Task DisconnectAsync_Closes_Connection()
        {
            var port = GetRequiredService<IPort>();
            await await MessagingContext.ConnectAsync(ConnectionSettings);
            Assert.True(port.IsOpen);

            await MessagingContext.DisconnectAsync();

            Assert.False(port.IsOpen);
        }
    }
}
