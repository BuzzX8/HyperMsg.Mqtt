using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class PublishComponentTests
    {
        private readonly Host host;
        private readonly PublishComponent publishComponent;
        private readonly IMessageObservable observable;
        private readonly CancellationTokenSource tokenSource;

        private PublishReceivedEventArgs receiveEventArgs;

        public PublishComponentTests()
        {
            var services = new ServiceCollection();
            services.AddMessagingServices();
            services.AddMqttServices(new MqttConnectionSettings("client-id"));
            host = new Host(services);            
            publishComponent = host.Services.GetRequiredService<PublishComponent>();
            publishComponent.PublishReceived += (s, e) => receiveEventArgs = e;
            observable = host.Services.GetRequiredService<IMessageObservable>();
            tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public async Task PublishAsync_Sends_Publish_Packet()
        {
            var request = CreatePublishRequest();
            var publishPacket = default(Publish);
            observable.OnTransmit<Publish>(p => publishPacket = p);

            await publishComponent.PublishAsync(request, tokenSource.Token);

            Assert.NotNull(publishPacket);
            Assert.Equal(request.TopicName, publishPacket.Topic);
            Assert.Equal(request.Message.ToArray(), publishPacket.Message.ToArray());
            Assert.Equal(request.Qos, publishPacket.Qos);
        }

        [Fact]
        public async Task PublishAsync_Sends_Qos0_Message_And_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos0);
            var publishPacket = default(Publish);
            observable.OnTransmit<Publish>(p => publishPacket = p);

            var task = await publishComponent.PublishAsync(request, tokenSource.Token);            
                        
            Assert.True(task.IsCompleted);
            Assert.NotNull(publishPacket);
        }

        [Fact]
        public async Task PublishAsync_Sends_Qos1_Message_And_Not_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var publishPacket = default(Publish);
            observable.OnTransmit<Publish>(p => publishPacket = p);

            var task = await publishComponent.PublishAsync(request, tokenSource.Token);
                        
            Assert.NotNull(publishPacket);
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public async Task Handle_Completes_Task_For_Qos1_Publish()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var publishPacket = default(Publish);
            observable.OnTransmit<Publish>(p => publishPacket = p);

            var task = await publishComponent.PublishAsync(request, tokenSource.Token);            
            publishComponent.Handle(new PubAck(publishPacket.Id));
            
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRel_After_Receiving_PubRec()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var publishPacket = default(Publish);
            observable.OnTransmit<Publish>(p => publishPacket = p);
            var pubRel = default(PubRel);
            observable.OnTransmit<PubRel>(p => pubRel = p);

            var task = await publishComponent.PublishAsync(request, tokenSource.Token);
            await publishComponent.HandleAsync(new PubRec(publishPacket.Id), tokenSource.Token);
            
            Assert.False(task.IsCompleted);
            Assert.NotNull(pubRel);
            Assert.Equal(publishPacket.Id, pubRel.Id);
        }

        [Fact]
        public async Task HandleAsync_Completes_Task_For_Qos2_After_Receiving_PubComp()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var publishPacket = default(Publish);
            observable.OnTransmit<Publish>(p => publishPacket = p);
            var task = await publishComponent.PublishAsync(request, tokenSource.Token);
                        
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
            var pubAck = default(PubAck);
            observable.OnTransmit<PubAck>(p => pubAck = p);

            await publishComponent.HandleAsync(publish, tokenSource.Token);
            
            Assert.Equal(publish.Id, pubAck.Id);
            Assert.NotNull(receiveEventArgs);
            Assert.NotNull(pubAck);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRec_When_Qos2_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);
            var pubRec = default(PubRec);
            observable.OnTransmit<PubRec>(p => pubRec = p);

            await publishComponent.HandleAsync(publish, tokenSource.Token);

            Assert.Null(receiveEventArgs);
            Assert.NotNull(pubRec);
            Assert.Equal(publish.Id, pubRec.Id);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubCom_And_Rises_PublishReceived_After_Receiving_PubRel_Packet()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);
            var pubCom = default(PubComp);
            observable.OnTransmit<PubComp>(p => pubCom = p);
            await publishComponent.HandleAsync(publish, tokenSource.Token);

            await publishComponent.HandleAsync(new PubRel(publish.Id), tokenSource.Token);
                        
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
