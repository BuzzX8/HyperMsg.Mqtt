using HyperMsg.Mqtt.Client;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class ConnectionTests : MqttClientIntegrationTestsBase
    {        

        public ConnectionTests()
        {
            HandlerRegistry.Register<Received<ConnAck>>(ack =>
            {

            });
        }

        [Fact]
        public async Task ConnectAsync_Establishes_Connection()
        {
            var sessionState = await ConnectAsync(true, CancellationToken.None);

            Assert.Equal(SessionState.Clean, sessionState);
            Assert.IsType<ConnAck>(LastResponse);
        }
    }
}