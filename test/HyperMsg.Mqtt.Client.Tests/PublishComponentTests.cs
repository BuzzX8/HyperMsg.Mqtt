using HyperMsg.Mqtt.Packets;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class PublishComponentTests
    {
        private readonly FakeMessageSender messageSender;
        private readonly PublishComponent publishComponent;

        private readonly CancellationTokenSource tokenSource;

        private PublishReceivedEventArgs receiveEventArgs;

        public PublishComponentTests()
        {
            messageSender = new FakeMessageSender();
            publishComponent = new PublishComponent(messageSender);
            publishComponent.PublishReceived += (s, e) => receiveEventArgs = e;
            tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public async Task PublishAsync_Sends_Publish_Packet()
        {
            var request = CreatePublishRequest();

            await publishComponent.PublishAsync(request, tokenSource.Token);

            var publishPacket = messageSender.GetLastTransmit<Publish>();

            Assert.NotNull(publishPacket);
            Assert.Equal(request.TopicName, publishPacket.Topic);
            Assert.Equal(request.Message.ToArray(), publishPacket.Message.ToArray());
            Assert.Equal(request.Qos, publishPacket.Qos);
        }

        [Fact]
        public void PublishAsync_Sends_Qos0_Message_And_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos0);

            var task = publishComponent.PublishAsync(request, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var sentMessage = messageSender.GetLastTransmit<Publish>();
            Assert.True(task.IsCompleted);
            Assert.NotNull(sentMessage);
        }

        [Fact]
        public void PublishAsync_Sends_Qos1_Message_And_Not_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);

            var task =  publishComponent.PublishAsync(request, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var publishPacket = messageSender.GetLastTransmit<Publish>();
            Assert.NotNull(publishPacket);
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public void Handle_Completes_Task_For_Qos1_Publish()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var task = publishComponent.PublishAsync(request, tokenSource.Token);
            messageSender.WaitMessageToSent();
            var publishPacket = messageSender.GetLastTransmit<Publish>();

            publishComponent.Handle(new PubAck(publishPacket.Id));
            messageSender.WaitMessageToSent();

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRel_After_Receiving_PubRec()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var task = publishComponent.PublishAsync(request, tokenSource.Token);
            messageSender.WaitMessageToSent();
            
            var publishPacket = messageSender.GetLastTransmit<Publish>();

            await publishComponent.HandleAsync(new PubRec(publishPacket.Id), tokenSource.Token);
            messageSender.WaitMessageToSent();

            Assert.False(task.IsCompleted);
            var pubRel = messageSender.GetLastTransmit<PubRel>();
            Assert.NotNull(pubRel);
            Assert.Equal(publishPacket.Id, pubRel.Id);
        }

        [Fact]
        public async Task HandleAsync_Completes_Task_For_Qos2_After_Receiving_PubComp()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var task = publishComponent.PublishAsync(request, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var publishPacket = messageSender.GetLastTransmit<Publish>();
            await publishComponent.HandleAsync(new PubRec(publishPacket.Id), tokenSource.Token);

            publishComponent.Handle(new PubComp(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleAsync_Rises_PublishReceived_When_Qos0_Publish_Received()
        {
            var publish = CreatePublishPacket();

            await publishComponent.HandleAsync(publish, tokenSource.Token);

            Assert.NotNull(receiveEventArgs);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubAck_And_Rises_PublishReceived_When_Qos1_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos1);

            await publishComponent.HandleAsync(publish, tokenSource.Token);
            messageSender.WaitMessageToSent();

            var pubAck = messageSender.GetLastTransmit<PubAck>();
            Assert.Equal(publish.Id, pubAck.Id);
            Assert.NotNull(receiveEventArgs);
            Assert.NotNull(pubAck);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRec_When_Qos2_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);

            await publishComponent.HandleAsync(publish, tokenSource.Token);

            var pubRec = messageSender.GetLastTransmit<PubRec>();
            Assert.Null(receiveEventArgs);
            Assert.NotNull(pubRec);
            Assert.Equal(publish.Id, pubRec.Id);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubCom_And_Rises_PublishReceived_After_Receiving_PubRel_Packet()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);
            await publishComponent.HandleAsync(publish, tokenSource.Token);

            await publishComponent.HandleAsync(new PubRel(publish.Id), tokenSource.Token);

            var pubCom = messageSender.GetLastTransmit<PubComp>();
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
