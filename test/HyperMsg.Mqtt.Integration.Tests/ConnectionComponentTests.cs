using HyperMsg.Mqtt.Client;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class ConnectionComponentTests : MqttComponentTestsBase
    {
        private readonly CancellationToken cancellationToken;

        public ConnectionComponentTests()
        {
            cancellationToken = new CancellationToken();
        }

        [Fact]
        public async Task ConnectAsync_Establishes_Connection()
        {
            var sessionState = await ConnectAsync(true, cancellationToken);

            Assert.Equal(SessionState.Clean, sessionState);
        }
    }
}
