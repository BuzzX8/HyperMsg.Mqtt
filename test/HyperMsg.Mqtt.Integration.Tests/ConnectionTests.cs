using HyperMsg.Mqtt.Client;
using HyperMsg.Transport;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class ConnectionTests : MqttClientIntegrationTestsBase
    {
        [Fact]
        public async Task ConnectAsync_Establishes_Connection()
        {
            var context = GetService<IMessagingContext>();
            await context.Sender.SendAsync(TransportCommand.Open, default);
            var sessionState = await await context.StartConnectAsync(ConnectionSettings, default);

            Assert.False(true);
        }
    }
}