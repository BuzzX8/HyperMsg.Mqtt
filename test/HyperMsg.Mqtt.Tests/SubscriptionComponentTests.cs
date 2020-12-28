using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class SubscriptionComponentTests
    {
        private readonly Host host;
        private readonly SubscriptionComponent component;
        private readonly IMessageObservable observable;
        private readonly CancellationTokenSource tokenSource;

        public SubscriptionComponentTests()
        {
            var services = new ServiceCollection();
            services.AddMessagingServices();
            services.AddMqttServices(new MqttConnectionSettings("test-client"));
            host = new Host(services);
            component = host.Services.GetRequiredService<SubscriptionComponent>();
            observable = host.Services.GetRequiredService<IMessageObservable>();
            tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public async Task SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var subscribePacket = default(Subscribe);
            observable.OnTransmit<Subscribe>(s => subscribePacket = s);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            await component.SubscribeAsync(request, tokenSource.Token);

            Assert.NotNull(subscribePacket);
        }

        [Fact]
        public async Task SubscribeAsync_Returns_SubscriptionResult_When_SubAck_Received()
        {
            var subscribePacket = default(Subscribe);
            observable.OnTransmit<Subscribe>(s => subscribePacket = s);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var task = await component.SubscribeAsync(request, tokenSource.Token);            
            var packetId = subscribePacket.Id;
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });

            component.Handle(subAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(subAck.Results, task.Result);
        }

        [Fact]
        public async Task UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var unsubscribe = default(Unsubscribe);
            observable.OnTransmit<Unsubscribe>(s => unsubscribe = s);
            var topics = new[] { "topic-1", "topic-2" };

            await component.UnsubscribeAsync(topics, tokenSource.Token);

            Assert.NotNull(unsubscribe);
            Assert.Equal(topics, unsubscribe.Topics);
        }

        [Fact]
        public async Task UnsubscribeAsync_Completes_Task_When_UnsubAck_Received()
        {
            var unsubscribe = default(Unsubscribe);
            observable.OnTransmit<Unsubscribe>(s => unsubscribe = s);
            var topics = new[] { "topic-1", "topic-2" };
            var task = await component.UnsubscribeAsync(topics, tokenSource.Token);

            component.Handle(new UnsubAck(unsubscribe.Id));

            Assert.True(task.IsCompleted);
        }
    }
}
