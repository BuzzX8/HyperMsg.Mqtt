using FakeItEasy;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClientTests
    {
        private readonly IConnectionController connectionController;
        private readonly IMessageSender<Packet> messageSender;
        private readonly MqttClient client;

        private readonly ManualResetEventSlim packetSentEvent = new ManualResetEventSlim();
        private readonly TimeSpan waitTimeout = TimeSpan.FromSeconds(2);

        private PublishReceivedEventArgs receiveEventArgs;
        private Packet sentPacket;

        public MqttClientTests()
        {
            connectionController = A.Fake<IConnectionController>();
            messageSender = A.Fake<IMessageSender<Packet>>();            
            client = new MqttClient(connectionController, messageSender);
            client.PublishReceived += (s, e) => receiveEventArgs = e;

            A.CallTo(() => messageSender.SendAsync(A<Packet>._, A<CancellationToken>._))
                .Invokes(foc =>
                {
                    sentPacket = foc.GetArgument<Packet>(0);
                    packetSentEvent.Set();
                })
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public void SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            _ = client.SubscribeAsync(request);
            packetSentEvent.Wait(waitTimeout);

            var subscribePacket = sentPacket as Subscribe;
            Assert.NotNull(subscribePacket);
        }

        [Fact]
        public async Task SubscribeAsync_Returns_SubscriptionResult_When_SubAck_Received()
        {
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var task = client.SubscribeAsync(request);
            packetSentEvent.Wait(waitTimeout);
            var packetId = ((Subscribe)sentPacket).Id;
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });

            await client.HandleAsync(subAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(subAck.Results, task.Result);
        }

        [Fact]
        public void UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var topics = new[] { "topic-1", "topic-2" };

            client.UnsubscribeAsync(topics);
            packetSentEvent.Wait(waitTimeout);

            var unsubscribe = sentPacket as Unsubscribe;

            Assert.NotNull(unsubscribe);
            Assert.Equal(topics, unsubscribe.Topics);
        }

        [Fact]
        public async Task UnsubscribeAsync_Completes_Task_When_UnsubAck_Received()
        {
            var topics = new[] { "topic-1", "topic-2" };
            var task = client.UnsubscribeAsync(topics);
            packetSentEvent.Wait(waitTimeout);
            var unsubscribe = sentPacket as Unsubscribe;

            await client.HandleAsync(new UnsubAck(unsubscribe.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void PublishAsync_Sends_Publish_Packet()
        {
            var request = CreatePublishRequest();

            _ = client.PublishAsync(request);
            packetSentEvent.Wait(waitTimeout);

            var publishPacket = sentPacket as Publish;

            Assert.NotNull(publishPacket);
            Assert.Equal(request.TopicName, publishPacket.Topic);
            Assert.Equal(request.Message.ToArray(), publishPacket.Message.ToArray());
            Assert.Equal(request.Qos, publishPacket.Qos);
        }

        [Fact]
        public void PublishAsync_Sends_Qos0_Message_And_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos0);

            var task = client.PublishAsync(request);
            packetSentEvent.Wait(waitTimeout);

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void PublishAsync_Sends_Qos1_Message_And_Not_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);

            var task = client.PublishAsync(request);
            packetSentEvent.Wait(waitTimeout);

            var publishPacket = sentPacket as Publish;
            Assert.NotNull(publishPacket);
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public void PingAsync_Sends_PingReq_Packet()
        {
            var task = client.PingAsync();

            var pingReq = sentPacket as PingReq;
            Assert.NotNull(pingReq);
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public async Task PingAsync_Completes_Task_When_PingResp_Received()
        {
            var task = client.PingAsync();
            packetSentEvent.Wait(waitTimeout);

            await client.HandleAsync(PingResp.Instance);

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Handle_Completes_Task_For_Qos1_Publish()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var task = client.PublishAsync(request);
            packetSentEvent.Wait(waitTimeout);
            var publishPacket = sentPacket as Publish;

            await client.HandleAsync(new PubAck(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Handle_Sends_PubRel_After_Receiving_PubRec()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var task = client.PublishAsync(request);
            packetSentEvent.Wait(waitTimeout);
            var publishPacket = sentPacket as Publish;

            await client.HandleAsync(new PubRec(publishPacket.Id));

            Assert.False(task.IsCompleted);
            var pubRel = sentPacket as PubRel;
            Assert.NotNull(pubRel);
            Assert.Equal(publishPacket.Id, pubRel.Id);
        }

        [Fact]
        public async Task Handle_Completes_Task_For_Qos2_After_Receiving_PubComp()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var task = client.PublishAsync(request);
            packetSentEvent.Wait(waitTimeout);
            var publishPacket = sentPacket as Publish;
            await client.HandleAsync(new PubRec(publishPacket.Id));

            await client.HandleAsync(new PubComp(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Handle_Rises_PublishReceived_When_Qos0_Publish_Received()
        {
            var publish = CreatePublishPacket();

            await client.HandleAsync(publish);

            Assert.NotNull(receiveEventArgs);            
        }

        [Fact]
        public async Task Handle_Sends_PubAck_And_Rises_PublishReceived_When_Qos1_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos1);

            await client.HandleAsync(publish);

            var pubAck = sentPacket as PubAck;
            Assert.Equal(publish.Id, pubAck.Id);
            Assert.NotNull(receiveEventArgs);
            Assert.NotNull(pubAck);
        }

        [Fact]
        public async Task Handle_Sends_PubRec_When_Qos2_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);

            await client.HandleAsync(publish);

            var pubRec = sentPacket as PubRec;
            Assert.Null(receiveEventArgs);
            Assert.NotNull(pubRec);
            Assert.Equal(publish.Id, pubRec.Id);
        }

        [Fact]
        public async Task Handle_Sends_PubCom_And_Rises_PublishReceived_After_Receiving_PubRel_Packet()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);
            await client.HandleAsync(publish);

            await client.HandleAsync(new PubRel(publish.Id));

            var pubCom = sentPacket as PubComp;
            Assert.NotNull(pubCom);
            Assert.NotNull(receiveEventArgs);
        }

        private PublishRequest CreatePublishRequest(QosLevel qos = QosLevel.Qos0)
        {
            var topicName = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            return new PublishRequest(topicName, message, qos);
        }

        private Publish CreatePublishPacket(QosLevel qos = QosLevel.Qos0)
        {
            var topicName = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            return new Publish(Guid.NewGuid().ToByteArray()[0], topicName, message, qos);
        }
    }
}