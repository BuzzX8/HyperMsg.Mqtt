using HyperMsg.Extensions;
using HyperMsg.Mqtt.Extensions;
using HyperMsg.Mqtt.Packets;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class PingTests : IntegrationTestBase
    {
        [Fact]
        public async Task PingAsync_Receives_Ping_Response()
        {
            var pingResp = default(PingResp);
            //MessageObservable.OnReceived<PingResp>(r => pingResp = r);
            await ConnectAsync();

            var pingTask = MessagingContext.PingAsync();

            pingTask.Wait(DefaultWaitTimeout);

            Assert.True(pingTask.IsCompleted);
            Assert.NotNull(pingResp);
        }
    }
}
