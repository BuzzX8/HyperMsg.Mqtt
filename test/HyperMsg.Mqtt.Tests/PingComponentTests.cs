using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class PingComponentTests
    {        
        private readonly Host host;
        private readonly CancellationTokenSource tokenSource;
        private readonly PingComponent pingComponent;
        private readonly IMessageObservable observable;

        public PingComponentTests()
        {
            var services = new ServiceCollection();
            services.AddMessagingServices();
            services.AddMqttServices(new MqttConnectionSettings("test-client"));
            host = new Host(services);
            tokenSource = new CancellationTokenSource();
            pingComponent = host.Services.GetRequiredService<PingComponent>();
            observable = host.Services.GetRequiredService<IMessageObservable>();
        }

        [Fact]
        public async Task PingAsync_Sends_PingReq_Packet()
        {
            var pingReq = default(PingReq);
            observable.OnTransmit<PingReq>(p => pingReq = p);
            
            await pingComponent.PingAsync(tokenSource.Token);
            
            Assert.NotNull(pingReq);
        }

        [Fact]
        public async Task PingAsync_Completes_Task_When_PingResp_Received()
        {
            var task = await pingComponent.PingAsync(tokenSource.Token);

            pingComponent.Handle(PingResp.Instance);

            Assert.True(task.IsCompleted);
        }
    }
}
