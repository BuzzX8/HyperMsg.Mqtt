using FakeItEasy;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class PublishComponentTests
    {
        private readonly IMessageSender<Packet> messageSender;
        private readonly PublishComponent publishComponent;

        private readonly CancellationToken cancellationToken;

        private PublishReceivedEventArgs receiveEventArgs;
        private Packet sentPacket;

        public PublishComponentTests()
        {
            messageSender = A.Fake<IMessageSender<Packet>>();
            publishComponent = new PublishComponent(messageSender);
            publishComponent.PublishReceived += (s, e) => receiveEventArgs = e;
            A.CallTo(() => messageSender.SendAsync(A<Packet>._, A<CancellationToken>._))
                .Invokes(foc => sentPacket = foc.GetArgument<Packet>(0))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public void PublishAsync_Sends_Publish_Packet()
        {
            var request = CreatePublishRequest();

            _ = publishComponent.PublishAsync(request, cancellationToken);

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

            var task = publishComponent.PublishAsync(request, cancellationToken);

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void PublishAsync_Sends_Qos1_Message_And_Not_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);

            var task = publishComponent.PublishAsync(request, cancellationToken);

            var publishPacket = sentPacket as Publish;
            Assert.NotNull(publishPacket);
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public void Handle_Completes_Task_For_Qos1_Publish()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var task = publishComponent.PublishAsync(request, cancellationToken);
            var publishPacket = sentPacket as Publish;

            publishComponent.Handle(new PubAck(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRel_After_Receiving_PubRec()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var task = publishComponent.PublishAsync(request, cancellationToken);
            
            var publishPacket = sentPacket as Publish;

            await publishComponent.HandleAsync(new PubRec(publishPacket.Id), cancellationToken);

            Assert.False(task.IsCompleted);
            var pubRel = sentPacket as PubRel;
            Assert.NotNull(pubRel);
            Assert.Equal(publishPacket.Id, pubRel.Id);
        }

        [Fact]
        public async Task HandleAsync_Completes_Task_For_Qos2_After_Receiving_PubComp()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var task = publishComponent.PublishAsync(request, cancellationToken);
            
            var publishPacket = sentPacket as Publish;
            await publishComponent.HandleAsync(new PubRec(publishPacket.Id), cancellationToken);

            publishComponent.Handle(new PubComp(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleAsync_Rises_PublishReceived_When_Qos0_Publish_Received()
        {
            var publish = CreatePublishPacket();

            await publishComponent.HandleAsync(publish, cancellationToken);

            Assert.NotNull(receiveEventArgs);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubAck_And_Rises_PublishReceived_When_Qos1_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos1);

            await publishComponent.HandleAsync(publish, cancellationToken);

            var pubAck = sentPacket as PubAck;
            Assert.Equal(publish.Id, pubAck.Id);
            Assert.NotNull(receiveEventArgs);
            Assert.NotNull(pubAck);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRec_When_Qos2_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);

            await publishComponent.HandleAsync(publish, cancellationToken);

            var pubRec = sentPacket as PubRec;
            Assert.Null(receiveEventArgs);
            Assert.NotNull(pubRec);
            Assert.Equal(publish.Id, pubRec.Id);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubCom_And_Rises_PublishReceived_After_Receiving_PubRel_Packet()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);
            await publishComponent.HandleAsync(publish, cancellationToken);

            await publishComponent.HandleAsync(new PubRel(publish.Id), cancellationToken);

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
