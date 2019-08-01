using HyperMsg.Mqtt.Client;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class ConnectionTests : MqttClientIntegrationTestsBase
    {
        private readonly CancellationToken cancellationToken;

        public ConnectionTests()
        {
            cancellationToken = new CancellationToken();
        }

        [Fact]
        public async Task ConnectAsync_Establishes_Connection()
        {
            var sessionState = await ConnectAsync(true, cancellationToken);

            Assert.Equal(SessionState.Clean, sessionState);
            Assert.IsType<ConnAck>(LastResponse);
        }
    }
}