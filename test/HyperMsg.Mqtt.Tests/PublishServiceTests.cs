using HyperMsg.Mqtt.Packets;
using System;
using Xunit;

namespace HyperMsg.Mqtt;

public class PublishServiceTests
{
    private readonly MessageBroker messageBroker;
    private readonly PublishService service;

    public PublishServiceTests()
    {
        messageBroker = new();
        service = new(messageBroker, messageBroker);
    }

    [Fact]
    public void Publish_Sends_Correct_Packet()
    {
        var topic = Guid.NewGuid().ToString();
        var message = Guid.NewGuid().ToByteArray();
        var qos = QosLevel.Qos1;
        var actualPacket = default(Publish);

        messageBroker.Register<Publish>(publish => actualPacket = publish);
        var packetId = service.Publish(topic, message, qos);

        Assert.NotNull(actualPacket);
        Assert.Equal(packetId, actualPacket.Id);
        Assert.Equal(topic, actualPacket.Topic);
        Assert.Equal(message, actualPacket.Message);
        Assert.Equal(qos, actualPacket.Qos);
    }

    [Fact]
    public void Publish_Does_Not_Stores_Publish_Packet_For_Qos0()
    {
        var packetId = service.Publish(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

        Assert.False(service.PendingPublications.ContainsKey(packetId));
    }

    [Fact]
    public void Publish_Stores_Publish_Packet_For_Qos1()
    {
        var packetId = service.Publish(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

        Assert.True(service.PendingPublications.ContainsKey(packetId));
    }

    [Fact]
    public void Receiving_PubAck_Invokes_Handler_For_Qos1()
    {        
        var topic = Guid.NewGuid().ToString();

        //messageBroker.Register<PublishCompletedHandlerArgs>(args => actualArgs = args);
        var packetId = service.Publish(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);
        messageBroker.Dispatch(new PubAck(packetId));

        //Assert.NotNull(actualArgs);
        //Assert.Equal(packetId, actualArgs.Id);
        //Assert.Equal(topic, actualArgs.Topic);
        //Assert.Equal(QosLevel.Qos1, actualArgs.Qos);
    }

    [Fact]
    public void Received_PubAck_Invokes_Async_Handler_For_Qos1()
    {
        var topic = Guid.NewGuid().ToString();
        
        var packetId = service.Publish(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);
        messageBroker.Dispatch(new PubAck(packetId));

        //Assert.NotNull(actualArgs);
        //Assert.Equal(packetId, actualArgs.Id);
        //Assert.Equal(topic, actualArgs.Topic);
        //Assert.Equal(QosLevel.Qos1, actualArgs.Qos);
    }

    [Fact]
    public void Receiving_PubRec_Transmits_PubRel()
    {
        var pubRel = default(PubRel);
        var topic = Guid.NewGuid().ToString();

        messageBroker.Register<PubRel>(packet => pubRel = packet);
        var packetId = service.Publish(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos2);
        messageBroker.Dispatch(new PubRec(packetId));

        Assert.NotNull(pubRel);
        Assert.Equal(packetId, pubRel.Id);
    }

    [Fact]
    public void Received_PubComp_Invokes_Handler_For_Qos2()
    {
        var topic = Guid.NewGuid().ToString();
                
        var packetId = service.Publish(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos2);
        messageBroker.Dispatch(new PubRec(packetId));
        messageBroker.Dispatch(new PubComp(packetId));

        //Assert.NotNull(actualArgs);
        //Assert.Equal(packetId, actualArgs.Id);
        //Assert.Equal(topic, actualArgs.Topic);
        //Assert.Equal(QosLevel.Qos2, actualArgs.Qos);
    }
}
