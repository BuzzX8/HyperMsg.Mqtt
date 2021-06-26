using FakeItEasy;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class MessagingContextExtensionsTests : ServiceHostFixture
    {
        //#region UnsubscribeAsync

        //[Fact]
        //public void UnsubscribeAsync_Sends_Unsubscription_Request()
        //{
        //    var unsubscribe = default(Unsubscribe);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        var message = MqttDeserializer.Deserialize(data, out var bytesConsumed);
        //        unsubscribe = message as Unsubscribe;
        //        return bytesConsumed;
        //    });
        //    var topics = new[] { "topic-1", "topic-2" };

        //    _ = MessagingContext.UnsubscribeAsync(topics);

        //    Assert.NotNull(unsubscribe);
        //    Assert.Equal(topics, unsubscribe.Topics);
        //}

        //[Fact]
        //public void UnsubscribeAsync_Completes_Task_When_UnsubAck_Received()
        //{
        //    var unsubscribe = default(Unsubscribe);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        var message = MqttDeserializer.Deserialize(data, out var bytesConsumed);
        //        unsubscribe = message as Unsubscribe;
        //        return bytesConsumed;
        //    });
        //    var topics = new[] { "topic-1", "topic-2" };
        //    var task = MessagingContext.UnsubscribeAsync(topics);

        //    MessageSender.SendWriteToBufferCommand(BufferType.Receiving, new UnsubAck(unsubscribe.Id));

        //    Assert.True(task.Completion.IsCompleted);
        //}

        //#endregion

        //#region PublishAsync

        //[Fact]
        //public void PublishAsync_Sends_Publish_Packet()
        //{
        //    var request = CreatePublishRequest();
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        publishPacket = MqttDeserializer.Deserialize(data, out var bytesConsumed) as Publish;
        //        return bytesConsumed;
        //    });

        //    _ = MessagingContext.PublishAsync(request);

        //    Assert.NotNull(publishPacket);
        //    Assert.Equal(request.TopicName, publishPacket.Topic);
        //    Assert.Equal(request.Message.ToArray(), publishPacket.Message.ToArray());
        //    Assert.Equal(request.Qos, publishPacket.Qos);
        //}

        //[Fact]
        //public void PublishAsync_Sends_Qos0_Message_And_Completes_Task()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos0);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        publishPacket = MqttDeserializer.Deserialize(data, out var bytesConsumed) as Publish;
        //        return bytesConsumed;
        //    });

        //    var task = MessagingContext.PublishAsync(request);

        //    Assert.True(task.Completion.IsCompleted);
        //    Assert.NotNull(publishPacket);
        //}

        //[Fact]
        //public void PublishAsync_Sends_Qos1_Message_And_Not_Completes_Task()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos1);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        publishPacket = MqttDeserializer.Deserialize(data, out var bytesConsumed) as Publish;
        //        return bytesConsumed;
        //    });

        //    var task = MessagingContext.PublishAsync(request);

        //    Assert.NotNull(publishPacket);
        //    Assert.False(task.Completion.IsCompleted);
        //}

        //[Fact]
        //public void Handle_Completes_Task_For_Qos1_Publish()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos1);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        publishPacket = MqttDeserializer.Deserialize(data, out var bytesConsumed) as Publish;
        //        return bytesConsumed;
        //    });

        //    var task = MessagingContext.PublishAsync(request);
        //    MessageSender.SendWriteToBufferCommand(BufferType.Receiving, new PubAck(publishPacket.Id));

        //    Assert.True(task.Completion.IsCompleted);
        //}

        //[Fact]
        //public void HandleAsync_Sends_PubRel_After_Receiving_PubRec()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos2);
        //    var publishPacket = default(Publish);
        //    var pubRel = default(PubRel);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        var packet = MqttDeserializer.Deserialize(data, out var bytesConsumed);

        //        if (packet is Publish publish)
        //        {
        //            publishPacket = publish;
        //        }
        //        else
        //        {
        //            pubRel = (PubRel)packet;
        //        }

        //        return bytesConsumed;
        //    });

        //    var task = MessagingContext.PublishAsync(request);
        //    MessageSender.SendWriteToBufferCommand(BufferType.Receiving, new PubRec(publishPacket.Id));

        //    Assert.False(task.Completion.IsCompleted);
        //    Assert.NotNull(pubRel);
        //    Assert.Equal(publishPacket.Id, pubRel.Id);
        //}

        //[Fact]
        //public void HandleAsync_Completes_Task_For_Qos2_After_Receiving_PubComp()
        //{
        //    var request = CreatePublishRequest(QosLevel.Qos2);
        //    var publishPacket = default(Publish);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        var packet = MqttDeserializer.Deserialize(data, out var bytesConsumed);

        //        if (packet is Publish publish)
        //        {
        //            publishPacket = publish;
        //        }

        //        return bytesConsumed;
        //    });
        //    var task = MessagingContext.PublishAsync(request);

        //    MessageSender.SendWriteToBufferCommand(BufferType.Receiving, new PubRec(publishPacket.Id));
        //    MessageSender.SendWriteToBufferCommand(BufferType.Receiving, new PubComp(publishPacket.Id));

        //    Assert.True(task.Completion.IsCompleted);
        //}

        //private static PublishRequest CreatePublishRequest(QosLevel qos = QosLevel.Qos0)
        //{
        //    var topicName = Guid.NewGuid().ToString();
        //    var message = Guid.NewGuid().ToByteArray();
        //    return new PublishRequest(topicName, message, qos);
        //}

        //#endregion

        //#region PingAsync

        //[Fact]
        //public void PingAsync_Sends_PingReq_Packet()
        //{
        //    var pingReq = default(PingReq);
        //    HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
        //    {
        //        pingReq = MqttDeserializer.Deserialize(data, out var bytesConsumed) as PingReq;
        //        return bytesConsumed;
        //    });

        //    _ = MessagingContext.PingAsync();

        //    Assert.NotNull(pingReq);
        //}

        //[Fact]
        //public void PingAsync_Completes_Task_When_PingResp_Received()
        //{
        //    var task = MessagingContext.PingAsync();

        //    MessageSender.SendWriteToBufferCommand(BufferType.Receiving, PingResp.Instance);

        //    Assert.True(task.Completion.IsCompleted);
        //}

        //#endregion
    }
}
