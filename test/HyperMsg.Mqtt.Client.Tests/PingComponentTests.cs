using System.Threading;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class PingComponentTests
    {
        private readonly FakeMessageSender messageSender;
        private readonly CancellationTokenSource tokenSource;
        private readonly PingComponent pingComponent;

        public PingComponentTests()
        {
            messageSender = new FakeMessageSender();
            tokenSource = new CancellationTokenSource();
            pingComponent = new PingComponent(messageSender);
        }

        [Fact]
        public void PingAsync_Sends_PingReq_Packet()
        {
            var task = pingComponent.PingAsync(tokenSource.Token);
            messageSender.WaitMessageToSent();

            var pingReq = messageSender.GetLastTransmit<PingReq>();
            Assert.NotNull(pingReq);
        }

        [Fact]
        public void PingAsync_Completes_Task_When_PingResp_Received()
        {
            var task = pingComponent.PingAsync(tokenSource.Token);

            pingComponent.Handle(PingResp.Instance);

            Assert.True(task.IsCompleted);
        }
    }
}
