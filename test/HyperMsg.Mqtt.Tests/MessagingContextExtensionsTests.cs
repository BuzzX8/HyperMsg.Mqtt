using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class MessagingContextExtensionsTests : ServiceHostFixture
    {        
        private readonly MqttConnectionSettings connectionSettings;
        private readonly CancellationTokenSource tokenSource;

        public MessagingContextExtensionsTests() : base(services => services.AddMqttServices())
        {
            connectionSettings = new MqttConnectionSettings("test-client");
            tokenSource = new CancellationTokenSource();
        }

        #region ConnectAsync

        [Fact]
        public void ConnectAsync_Sends_Open_TransportCommand()
        {
            var wasInvoked = false;

            HandlersRegistry.RegisterOpenConnectionCommandHandler(() => wasInvoked = true);
            MessagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.True(wasInvoked);
        }

        [Fact]
        public void ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;
            var wasInvoked = false;
            HandlersRegistry.RegisterSetTlsCommandHandler(() => wasInvoked = true);
            MessagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.True(wasInvoked);
        }

        [Fact]
        public void ConnectAsync_Sends_Correct_Packet()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId
            };

            VerifyTransmittedConnectPacket(expectedPacket);
        }

        private void VerifyTransmittedConnectPacket(Connect expectedPacket)
        {
            var actualPacket = default(Connect);
            HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
            {
                var message = MqttDeserializer.Deserialize(data, out var bytesConsumed);
                actualPacket = message as Connect;
                return bytesConsumed;
            });
            MessagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void Received_Connack_Completes_Connect_Task_With_Correct_Result_For_SessionState()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = MessagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            MessageSender.SendWriteToBufferCommand(BufferType.Receiving, connAck);

            Assert.True(task.Completion.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Completion.Result);
        }

        [Fact]
        public void Received_ConAck_Completes_Connect_Task_And_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = MessagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            MessageSender.SendWriteToBufferCommand(BufferType.Receiving, connAck);

            Assert.True(task.Completion.IsCompleted);
            Assert.Equal(SessionState.Present, task.Completion.Result);
        }

        #endregion

        #region SubscribeAsync

        [Fact]
        public void SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var subscribePacket = default(Subscribe);
            HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
            {
                var message = MqttDeserializer.Deserialize(data, out var bytesConsumed);
                subscribePacket = message as Subscribe;
                return bytesConsumed;
            });
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();

            _ = MessagingContext.SubscribeAsync(request, tokenSource.Token);

            Assert.NotNull(subscribePacket);
        }

        [Fact]
        public void SubscribeAsync_Returns_SubscriptionResult_When_SubAck_Received()
        {
            var subscribePacket = default(Subscribe);
            HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
            {
                var message = MqttDeserializer.Deserialize(data, out var bytesConsumed);
                subscribePacket = message as Subscribe;
                return bytesConsumed;
            });
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var task = MessagingContext.SubscribeAsync(request, tokenSource.Token);
            var packetId = subscribePacket.Id;
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });

            MessageSender.SendWriteToBufferCommand(BufferType.Receiving, subAck);

            Assert.True(task.Completion.IsCompleted);
            Assert.Equal(subAck.Results, task.Completion.Result);
        }

        #endregion

        //#region UnsubscribeAsync

        //[Fact]
        //public async Task UnsubscribeAsync_Sends_Unsubscription_Request()
        //{
        //    var unsubscribe = default(Unsubscribe);
        //    HandlersRegistry.RegisterTransmitHandler<Unsubscribe>(s => unsubscribe = s);
        //    var topics = new[] { "topic-1", "topic-2" };

        //    await MessagingContext.UnsubscribeAsync(topics, tokenSource.Token);

        //    Assert.NotNull(unsubscribe);
        //    Assert.Equal(topics, unsubscribe.Topics);
        //}

        //[Fact]
        //public async Task UnsubscribeAsync_Completes_Task_When_UnsubAck_Received()
        //{
        //    var unsubscribe = default(Unsubscribe);
        //    HandlersRegistry.RegisterTransmitHandler<Unsubscribe>(s => unsubscribe = s);
        //    var topics = new[] { "topic-1", "topic-2" };
        //    var task = await MessagingContext.UnsubscribeAsync(topics, tokenSource.Token);

        //    MessageSender.Receive(new UnsubAck(unsubscribe.Id));

        //    Assert.True(task.IsCompleted);
        //}

        //#endregion

        //#region PublishAsync

        //[Fact]
        //public async Task PublishAsync_Sends_Publish_Packet()
        //{
        //    var request = CreatePublishRequest();
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterTransmitHandler<Publish>(p => publishPacket = p);

        //    await MessagingContext.PublishAsync(request, tokenSource.Token);

        //    Assert.NotNull(publishPacket);
        //    Assert.Equal(request.TopicName, publishPacket.Topic);
        //    Assert.Equal(request.Message.ToArray(), publishPacket.Message.ToArray());
        //    Assert.Equal(request.Qos, publishPacket.Qos);
        //}

        //[Fact]
        //public async Task PublishAsync_Sends_Qos0_Message_And_Completes_Task()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos0);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterTransmitHandler<Publish>(p => publishPacket = p);

        //    var task = await MessagingContext.PublishAsync(request, tokenSource.Token);

        //    Assert.True(task.IsCompleted);
        //    Assert.NotNull(publishPacket);
        //}

        //[Fact]
        //public async Task PublishAsync_Sends_Qos1_Message_And_Not_Completes_Task()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos1);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterTransmitHandler<Publish>(p => publishPacket = p);

        //    var task = await MessagingContext.PublishAsync(request, tokenSource.Token);

        //    Assert.NotNull(publishPacket);
        //    Assert.False(task.IsCompleted);
        //}

        //[Fact]
        //public async Task Handle_Completes_Task_For_Qos1_Publish()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos1);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterTransmitHandler<Publish>(p => publishPacket = p);

        //    var task = await MessagingContext.PublishAsync(request, tokenSource.Token);
        //    MessageSender.Receive(new PubAck(publishPacket.Id));

        //    Assert.True(task.IsCompleted);
        //}

        //[Fact]
        //public async Task HandleAsync_Sends_PubRel_After_Receiving_PubRec()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos2);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterTransmitHandler<Publish>(p => publishPacket = p);
        //    var pubRel = default(PubRel);
        //    HandlersRegistry.RegisterTransmitHandler<PubRel>(p => pubRel = p);

        //    var task = await MessagingContext.PublishAsync(request, tokenSource.Token);
        //    await MessagingContext.Sender.ReceiveAsync(new PubRec(publishPacket.Id), tokenSource.Token);

        //    Assert.False(task.IsCompleted);
        //    Assert.NotNull(pubRel);
        //    Assert.Equal(publishPacket.Id, pubRel.Id);
        //}

        //[Fact]
        //public async Task HandleAsync_Completes_Task_For_Qos2_After_Receiving_PubComp()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos2);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterTransmitHandler<Publish>(p => publishPacket = p);
        //    var task = await MessagingContext.PublishAsync(request, tokenSource.Token);

        //    await MessageSender.ReceiveAsync(new PubRec(publishPacket.Id), tokenSource.Token);

        //    MessageSender.Receive(new PubComp(publishPacket.Id));

        //    Assert.True(task.IsCompleted);
        //}

        //private PublishRequest CreatePublishRequest(QosLevel qos = QosLevel.Qos0)
        //{
        //    var topicName = Guid.NewGuid().ToString();
        //    var message = Guid.NewGuid().ToByteArray();
        //    return new PublishRequest(topicName, message, qos);
        //}

        //#endregion

        //#region PingAsync

        //[Fact]
        //public async Task PingAsync_Sends_PingReq_Packet()
        //{
        //    var pingReq = default(PingReq);
        //    HandlersRegistry.RegisterTransmitHandler<PingReq>(p => pingReq = p);

        //    await MessagingContext.PingAsync(tokenSource.Token);

        //    Assert.NotNull(pingReq);
        //}

        //[Fact]
        //public async Task PingAsync_Completes_Task_When_PingResp_Received()
        //{
        //    var task = await MessagingContext.PingAsync(tokenSource.Token);

        //    MessageSender.Receive(PingResp.Instance);

        //    Assert.True(task.IsCompleted);
        //}

        //#endregion
    }
}
