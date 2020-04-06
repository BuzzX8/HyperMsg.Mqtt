using HyperMsg.Mqtt.Client;
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
            var connAck = default(ConnAck);
            var observable = GetService<IMessageObservable>();
            observable.Subscribe<Received<ConnAck>>(r => connAck = r);
            var sessionState = await ConnectAsync(true, CancellationToken.None);

            Assert.Equal(SessionState.Clean, sessionState);
            Assert.NotNull(connAck);
        }
    }
}