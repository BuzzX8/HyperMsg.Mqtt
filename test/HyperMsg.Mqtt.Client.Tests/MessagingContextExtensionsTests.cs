using FakeItEasy;
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
    }
}
