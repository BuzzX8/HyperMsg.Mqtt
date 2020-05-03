using System;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class MessagingContextExtensionsTests
    {
        private readonly MessageBroker broker = new MessageBroker();
        private readonly MqttConnectionSettings connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());

        [Fact]
        public async Task StartConnectAsync_Sends_Connect_Message()
        {
            var message = default(Connect);
            broker.OnTransmit<Connect>(m => message = m);

            await broker.StartConnectAsync(connectionSettings, default);

            Assert.NotNull(message);
        }

        [Fact]
        public async Task Received_ConAck_Completes_Task_With_Correct_Result()
        {
            var task = await broker.StartConnectAsync(connectionSettings, default);

            broker.Received(new ConnAck(ConnectionResult.Accepted));

            Assert.True(task.Completion.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Completion.Result);
        }

        [Fact]
        public async Task StartSubscriptionAsync_Sends_Subscribe_Message()
        {
            var message = default(Subscribe);
            broker.OnTransmit<Subscribe>(m => message = m);
            var request = new[] { (Guid.NewGuid().ToString(), QosLevel.Qos0) };

            await broker.StartSubscriptionAsync(request, default);

            Assert.NotNull(message);
            Assert.Equal(request, message.Subscriptions);
        }

        [Fact]
        public async Task Received_SubAck_Completes_Task_With_Corect_Result()
        {
            var message = default(Subscribe);
            broker.OnTransmit<Subscribe>(m => message = m);
            var task = await broker.StartSubscriptionAsync(new[] { (Guid.NewGuid().ToString(), QosLevel.Qos0) }, default);
            var expected = new[] { SubscriptionResult.SuccessQos0 };

            broker.Received(new SubAck(message.Id, expected));

            Assert.True(task.Completion.IsCompleted);
            Assert.Equal(expected, task.Completion.Result);
        }
    }
}
