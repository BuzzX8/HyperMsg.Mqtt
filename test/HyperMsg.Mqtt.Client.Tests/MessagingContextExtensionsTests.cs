using FakeItEasy.Sdk;
using System;
using System.Linq;
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

        [Fact]
        public async Task StartUnsubscriptionAsync_Sends_Unsubscribe_Message()
        {
            var message = default(Unsubscribe);
            broker.OnTransmit<Unsubscribe>(m => message = m);
            var topics = Enumerable.Range(0, 9).Select(i => Guid.NewGuid().ToString()).ToArray();

            await broker.StartUnsubscriptionAsync(topics, default);

            Assert.NotNull(message);
            Assert.Equal(topics, message.Topics);
        }

        [Fact]
        public async Task Received_UnsubAck_Completes_Task_With_Correct_Result()
        {
            var message = default(Unsubscribe);
            broker.OnTransmit<Unsubscribe>(m => message = m);
            var topics = Enumerable.Range(0, 9).Select(i => Guid.NewGuid().ToString()).ToArray();
            var task = await broker.StartUnsubscriptionAsync(topics, default);

            broker.Received(new UnsubAck(message.Id));

            Assert.True(task.Completion.IsCompleted);
        }

        [Fact]
        public async Task StartPublishAsync_Sends_Publish_Message_And_Completes_Task_For_Qos0()
        {
            var request = CreatePublishRequest(QosLevel.Qos0);
            var message = default(Publish);
            broker.OnTransmit<Publish>(m => message = m);

            var task = await broker.StartPublishAsync(request, default);

            Assert.NotNull(message);
            Assert.True(task.Completion.IsCompleted);
        }

        [Fact]
        public async Task Received_PubAck_Completes_Task_For_Qos1_Publish()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var message = default(Publish);
            broker.OnTransmit<Publish>(m => message = m);
            var task = await broker.StartPublishAsync(request, default);
            Assert.False(task.Completion.IsCompleted);

            broker.Received(new PubAck(message.Id));

            Assert.True(task.Completion.IsCompleted);
        }

        [Fact]
        public async Task Received_PubRec_Transmits_PubRel()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var publish = default(Publish);
            var message = default(PubRel);
            broker.OnTransmit<Publish>(m => publish = m);
            broker.OnTransmit<PubRel>(m => message = m);

            var task = await broker.StartPublishAsync(request, default);
            broker.Received(new PubRec(publish.Id));

            Assert.NotNull(message);
            Assert.False(task.Completion.IsCompleted);
        }

        private PublishRequest CreatePublishRequest(QosLevel qosLevel) => new PublishRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), qosLevel);
    }
}