using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class PingTests : MqttClientIntegrationTestsBase
    {
        private readonly CancellationToken cancellationToken;

        public PingTests()
        {            
            cancellationToken = new CancellationToken();
        }

        [Fact]
        public async Task PingAsync_Receives_PingResp()
        {
            //await ConnectAsync(false, cancellationToken);

            //await Client.PingAsync(cancellationToken);

            //Assert.NotNull(LastResponse as PingResp);
        }
    }
}
