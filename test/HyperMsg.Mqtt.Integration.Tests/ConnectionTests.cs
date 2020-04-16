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
        public void ConnectAsync_Establishes_Connection()
        {
            Assert.False(true);
        }
    }
}