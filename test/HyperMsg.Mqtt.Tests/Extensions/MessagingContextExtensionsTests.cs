﻿using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Connection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Extensions
{
    public class MessagingContextExtensionsTests
    {
        private readonly ServiceHost host;
        private readonly MqttConnectionSettings connectionSettings;
        private readonly IMessagingContext messagingContext;
        private readonly IMessageHandlersRegistry observable;

        private readonly CancellationTokenSource tokenSource;

        public MessagingContextExtensionsTests()
        {
            connectionSettings = new MqttConnectionSettings("test-client");

            host = ServiceHost.CreateDefault(services => services.AddMqttServices());
            host.StartAsync().Wait();

            messagingContext = host.GetRequiredService<IMessagingContext>();
            observable = host.GetRequiredService<IMessageHandlersRegistry>();
            tokenSource = new CancellationTokenSource();
        }

        #region ConnectAsync

        [Fact]
        public async Task ConnectAsync_Sends_Open_TransportCommand()
        {
            var command = default(ConnectionCommand);
            observable.RegisterHandler<ConnectionCommand>(c => command = c);
            await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.Equal(ConnectionCommand.Open, command);
        }

        [Fact]
        public async Task ConnectAsync_Sends_SetTransportLevelSecurity_TransportCommand_If_UseTls_Is_True()
        {
            connectionSettings.UseTls = true;
            var command = default(ConnectionCommand);
            observable.RegisterHandler<ConnectionCommand>(c => command = c);
            await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.Equal(ConnectionCommand.SetTransportLevelSecurity, command);
        }

        [Fact]
        public async Task ConnectAsync_Sends_Correct_Packet()
        {
            var expectedPacket = new Connect
            {
                ClientId = connectionSettings.ClientId
            };

            await VerifyTransmittedConnectPacket(expectedPacket);
        }

        private async Task VerifyTransmittedConnectPacket(Connect expectedPacket)
        {
            var actualPacket = default(Connect);
            observable.RegisterTransmitHandler<Connect>(c => actualPacket = c);
            await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public async Task Received_Connack_Completes_Connect_Task_With_Correct_Result_For_SessionState()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted);
            var task = await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            messagingContext.Sender.Receive(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Clean, task.Result);
        }

        [Fact]
        public async Task Received_ConAck_Completes_Connect_Task_And_Returns_Correct_Result_For_Present_Session()
        {
            var connAck = new ConnAck(ConnectionResult.Accepted, true);
            var task = await messagingContext.ConnectAsync(connectionSettings, tokenSource.Token);

            messagingContext.Sender.Receive(connAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(SessionState.Present, task.Result);
        }

        #endregion

        #region SubscribeAsync

        [Fact]
        public async Task SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var subscribePacket = default(Subscribe);
            observable.RegisterTransmitHandler<Subscribe>(s => subscribePacket = s);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            await messagingContext.SubscribeAsync(request, tokenSource.Token);

            Assert.NotNull(subscribePacket);
        }

        [Fact]
        public async Task SubscribeAsync_Returns_SubscriptionResult_When_SubAck_Received()
        {
            var subscribePacket = default(Subscribe);
            observable.RegisterTransmitHandler<Subscribe>(s => subscribePacket = s);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();
            var task = await messagingContext.SubscribeAsync(request, tokenSource.Token);
            var packetId = subscribePacket.Id;
            var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });

            messagingContext.Sender.Receive(subAck);

            Assert.True(task.IsCompleted);
            Assert.Equal(subAck.Results, task.Result);
        }

        #endregion

        #region UnsubscribeAsync

        [Fact]
        public async Task UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var unsubscribe = default(Unsubscribe);
            observable.RegisterTransmitHandler<Unsubscribe>(s => unsubscribe = s);
            var topics = new[] { "topic-1", "topic-2" };

            await messagingContext.UnsubscribeAsync(topics, tokenSource.Token);

            Assert.NotNull(unsubscribe);
            Assert.Equal(topics, unsubscribe.Topics);
        }

        [Fact]
        public async Task UnsubscribeAsync_Completes_Task_When_UnsubAck_Received()
        {
            var unsubscribe = default(Unsubscribe);
            observable.RegisterTransmitHandler<Unsubscribe>(s => unsubscribe = s);
            var topics = new[] { "topic-1", "topic-2" };
            var task = await messagingContext.UnsubscribeAsync(topics, tokenSource.Token);

            messagingContext.Sender.Receive(new UnsubAck(unsubscribe.Id));

            Assert.True(task.IsCompleted);
        }

        #endregion

        #region PublishAsync

        [Fact]
        public async Task PublishAsync_Sends_Publish_Packet()
        {
            var request = CreatePublishRequest();
            var publishPacket = default(Publish);
            observable.RegisterTransmitHandler<Publish>(p => publishPacket = p);

            await messagingContext.PublishAsync(request, tokenSource.Token);

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
            observable.RegisterTransmitHandler<Publish>(p => publishPacket = p);

            var task = await messagingContext.PublishAsync(request, tokenSource.Token);

            Assert.True(task.IsCompleted);
            Assert.NotNull(publishPacket);
        }

        [Fact]
        public async Task PublishAsync_Sends_Qos1_Message_And_Not_Completes_Task()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var publishPacket = default(Publish);
            observable.RegisterTransmitHandler<Publish>(p => publishPacket = p);

            var task = await messagingContext.PublishAsync(request, tokenSource.Token);

            Assert.NotNull(publishPacket);
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public async Task Handle_Completes_Task_For_Qos1_Publish()
        {
            var request = CreatePublishRequest(QosLevel.Qos1);
            var publishPacket = default(Publish);
            observable.RegisterTransmitHandler<Publish>(p => publishPacket = p);

            var task = await messagingContext.PublishAsync(request, tokenSource.Token);
            messagingContext.Sender.Receive(new PubAck(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleAsync_Sends_PubRel_After_Receiving_PubRec()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var publishPacket = default(Publish);
            observable.RegisterTransmitHandler<Publish>(p => publishPacket = p);
            var pubRel = default(PubRel);
            observable.RegisterTransmitHandler<PubRel>(p => pubRel = p);

            var task = await messagingContext.PublishAsync(request, tokenSource.Token);
            await messagingContext.Sender.ReceiveAsync(new PubRec(publishPacket.Id), tokenSource.Token);

            Assert.False(task.IsCompleted);
            Assert.NotNull(pubRel);
            Assert.Equal(publishPacket.Id, pubRel.Id);
        }

        [Fact]
        public async Task HandleAsync_Completes_Task_For_Qos2_After_Receiving_PubComp()
        {
            var request = CreatePublishRequest(QosLevel.Qos2);
            var publishPacket = default(Publish);
            observable.RegisterTransmitHandler<Publish>(p => publishPacket = p);
            var task = await messagingContext.PublishAsync(request, tokenSource.Token);

            await messagingContext.Sender.ReceiveAsync(new PubRec(publishPacket.Id), tokenSource.Token);

            messagingContext.Sender.Receive(new PubComp(publishPacket.Id));

            Assert.True(task.IsCompleted);
        }

        private PublishRequest CreatePublishRequest(QosLevel qos = QosLevel.Qos0)
        {
            var topicName = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToByteArray();
            return new PublishRequest(topicName, message, qos);
        }

        #endregion

        #region PingAsync

        [Fact]
        public async Task PingAsync_Sends_PingReq_Packet()
        {
            var pingReq = default(PingReq);
            observable.RegisterTransmitHandler<PingReq>(p => pingReq = p);

            await messagingContext.PingAsync(tokenSource.Token);

            Assert.NotNull(pingReq);
        }

        [Fact]
        public async Task PingAsync_Completes_Task_When_PingResp_Received()
        {
            var task = await messagingContext.PingAsync(tokenSource.Token);

            messagingContext.Sender.Receive(PingResp.Instance);

            Assert.True(task.IsCompleted);
        }

        #endregion
    }
}