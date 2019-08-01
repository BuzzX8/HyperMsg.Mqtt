using HyperMsg.Mqtt.Client;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class PingComponentTests : MqttComponentTestsBase
    {
        private readonly PingComponent pingComponent;
        private readonly CancellationToken cancellationToken;

        public PingComponentTests()
        {
            pingComponent = new PingComponent(MessageSender);
            cancellationToken = new CancellationToken();
            HandlerRegistry.Register(p =>
            {
                if (p is PingResp pingResp)
                {
                    pingComponent.Handle(pingResp);
                }
            });
        }

        [Fact]
        public async Task Ping_Receives_PingResp()
        {
            await ConnectAsync(false, cancellationToken);

            await pingComponent.PingAsync(cancellationToken);

            Assert.NotNull(LastResponse as PingResp);
        }
    }
}
