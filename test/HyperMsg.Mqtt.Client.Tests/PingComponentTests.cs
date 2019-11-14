using FakeItEasy;
using System.Threading;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class PingComponentTests
    {
        private readonly IMessageSender messageSender;
        private readonly CancellationToken cancellationToken;
        private readonly PingComponent pingComponent;

        public PingComponentTests()
        {
            messageSender = A.Fake<IMessageSender>();
            cancellationToken = new CancellationToken();
            pingComponent = new PingComponent(messageSender);
        }

        [Fact]
        public void PingAsync_Sends_PingReq_Packet()
        {
            var task = pingComponent.PingAsync(cancellationToken);

            A.CallTo(() => messageSender.SendAsync(PingReq.Instance, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void PingAsync_Completes_Task_When_PingResp_Received()
        {
            var task = pingComponent.PingAsync(cancellationToken);

            pingComponent.Handle(PingResp.Instance);

            Assert.True(task.IsCompleted);
        }
    }
}
