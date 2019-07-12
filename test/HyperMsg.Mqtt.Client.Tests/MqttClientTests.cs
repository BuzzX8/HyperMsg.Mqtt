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
        private readonly AsyncHandler<TransportCommand> transportCommandHandler;
        private readonly IMessageSender<Packet> messageSender;
        private readonly MqttConnectionSettings connectionSettings;
        private readonly MqttClient client;

        private readonly CancellationToken cancellationToken = new CancellationToken();
        private readonly ManualResetEventSlim packetSentEvent = new ManualResetEventSlim();
        private readonly TimeSpan waitTimeout = TimeSpan.FromSeconds(2);

        private PublishReceivedEventArgs receiveEventArgs;
        private Packet sentPacket;

        public MqttClientTests()
        {
            transportCommandHandler = A.Fake<AsyncHandler<TransportCommand>>();
            messageSender = A.Fake<IMessageSender<Packet>>();
            connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            client = new MqttClient(transportCommandHandler, messageSender, connectionSettings);
            client.PublishReceived += (s, e) => receiveEventArgs = e;

            A.CallTo(() => messageSender.SendAsync(A<Packet>._, A<CancellationToken>._))
                .Invokes(foc =>
                {
                    sentPacket = foc.GetArgument<Packet>(0);
                    packetSentEvent.Set();
                })
                .Returns(Task.CompletedTask);
        }

        #region Connection tests

        [Fact]
        public void ConnectAsync_Sends_Open_TransportCommand()
        {
            _ = client.ConnectAsync(false, cancellationToken);
            packetSentEvent.Wait(waitTimeout);

            A.CallTo(() => transportCommandHandler.Invoke(TransportCommand.Open, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;

            _ = client.ConnectAsync(false, cancellationToken);
            packetSentEvent.Wait(waitTimeout);

            A.CallTo(() => transportCommandHandler.Invoke(TransportCommand.SetTransportLevelSecurity, cancellationToken)).MustHaveHappened();
        }

        [Fact]
        public void ConnectAsync_Sends_Correct_Packet()
        {
            _ = client.ConnectAsync();
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.False(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
            Assert.Equal(connectionSettings.ClientId, connPacket.ClientId);
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_CleanSession_Flag()
        {
            _ = client.ConnectAsync(true);
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.True(connPacket.Flags.HasFlag(ConnectFlags.CleanSession));
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_KeepAlive_Specified_In_Settings()
        {
            connectionSettings.KeepAlive = 0x9080;
            _ = client.ConnectAsync();
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;
            Assert.NotNull(connPacket);
            Assert.Equal(connectionSettings.KeepAlive, connPacket.KeepAlive);
        }

        [Fact]
        public void ConnectAsync_Sends_Connect_Packet_With_Corredt_WillMessageSettings()
        {
            var willTopic = Guid.NewGuid().ToString();
            var willMessage = Guid.NewGuid().ToByteArray();
            connectionSettings.WillMessageSettings = new WillMessageSettings(willTopic, willMessage, true);

            _ = client.ConnectAsync();
            packetSentEvent.Wait(waitTimeout);

            var connPacket = sentPacket as Connect;

            Assert.True(connPacket.Flags.HasFlag(ConnectFlags.Will));
            Assert.Equal(willTopic, connPacket.WillTopic);
            Assert.Equal(willMessage, connPacket.WillMessage);
        }

        [Fact]
        public void ConnectAsync_Returns_Correct_Result_For_Clean_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = client.ConnectAsync();

            packetSentEvent.Wait(waitTimeout);
            client.HandleAsync(connAck, cancellationToken);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Result);
        }

        [Fact]
        public void ConnectAsync_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = client.ConnectAsync();

            packetSentEvent.Wait(waitTimeout);
            client.HandleAsync(connAck, cancellationToken);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Present, task.Result);
        }

        #endregion

        #region Subscription tests

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

        #endregion

        #region Publish tests

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

        #endregion

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

        #region HandleAsync tests

        [Fact]
        public async Task HandleAsync_Completes_Task_For_Qos1_Publish()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var task = client.PublishAsync(request);
            packetSentEvent.Wait(waitTimeout);
            var publishPacket = sentPacket as Publish;

            await client.HandleAsync(new PubAck(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRel_After_Receiving_PubRec()
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
        public async Task HandleAsync_Completes_Task_For_Qos2_After_Receiving_PubComp()
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
        public async Task HandleAsync_Rises_PublishReceived_When_Qos0_Publish_Received()
        {
            var publish = CreatePublishPacket();

            await client.HandleAsync(publish);

            Assert.NotNull(receiveEventArgs);            
        }

        [Fact]
        public async Task HandleAsync_Sends_PubAck_And_Rises_PublishReceived_When_Qos1_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos1);

            await client.HandleAsync(publish);

            var pubAck = sentPacket as PubAck;
            Assert.Equal(publish.Id, pubAck.Id);
            Assert.NotNull(receiveEventArgs);
            Assert.NotNull(pubAck);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRec_When_Qos2_Publish_Received()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);

            await client.HandleAsync(publish);

            var pubRec = sentPacket as PubRec;
            Assert.Null(receiveEventArgs);
            Assert.NotNull(pubRec);
            Assert.Equal(publish.Id, pubRec.Id);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubCom_And_Rises_PublishReceived_After_Receiving_PubRel_Packet()
        {
            var publish = CreatePublishPacket(QosLevel.Qos2);
            await client.HandleAsync(publish);

            await client.HandleAsync(new PubRel(publish.Id));

            var pubCom = sentPacket as PubComp;
            Assert.NotNull(pubCom);
            Assert.NotNull(receiveEventArgs);
        }

        #endregion

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