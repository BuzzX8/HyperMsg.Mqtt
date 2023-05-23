using FakeItEasy;
using HyperMsg.Mqtt.Client.Internal;
using HyperMsg.Mqtt.Packets;
using Xunit;

namespace HyperMsg.Mqtt.Client.Tests.Internal;

public class PublishingTests
{
    private readonly Publishing publishing;
    private readonly Func<Packet, CancellationToken, Task> sendAction;

    public PublishingTests()
    {
        sendAction = A.Fake<Func<Packet, CancellationToken, Task>>();
        publishing = new(sendAction);
    }

    [Fact]
    public async Task Publish_Sends_Correct_Packet()
    {
        var topic = Guid.NewGuid().ToString();
        var message = Guid.NewGuid().ToByteArray();
        var qos = QosLevel.Qos1;
        var actualPacket = default(Publish);
        
        var packetId = await publishing.PublishAsync(topic, message, qos);

        Assert.NotNull(actualPacket);
        Assert.Equal(packetId, actualPacket.Id);
        Assert.Equal(topic, actualPacket.TopicName);
        Assert.Equal(message, actualPacket.Payload);
        Assert.Equal(qos, actualPacket.Qos);
    }

    [Fact]
    public async Task Publish_Does_Not_Stores_Publish_Packet_For_Qos0()
    {
        var packetId = await publishing.PublishAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos0);

        Assert.False(publishing.PendingPublications.ContainsKey(packetId));
    }

    [Fact]
    public async Task Publish_Stores_Publish_Packet_For_Qos1()
    {
        var packetId = await publishing.PublishAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

        Assert.True(publishing.PendingPublications.ContainsKey(packetId));
    }

    [Fact]
    public async Task Receiving_PubAck_Invokes_Handler_For_Qos1()
    {
        var topic = Guid.NewGuid().ToString();

        var packetId = await publishing.PublishAsync(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos1);

        Assert.False(publishing.PendingPublications.ContainsKey(packetId));
    }

    [Fact]
    public async Task Receiving_PubRec_Transmits_PubRel()
    {
        var pubRel = default(PubRel);
        var topic = Guid.NewGuid().ToString();
        
        var packetId = await publishing.PublishAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToByteArray(), QosLevel.Qos2);

        Assert.Equal(packetId, pubRel.Id);
        Assert.Contains(pubRel.Id, publishing.ReleasedPublications);
    }

    [Fact]
    public async Task Received_PubComp_Invokes_Handler_For_Qos2()
    {
        var topic = Guid.NewGuid().ToString();

        var packetId = await publishing.PublishAsync(topic, Guid.NewGuid().ToByteArray(), QosLevel.Qos2);

        Assert.False(publishing.PendingPublications.ContainsKey(packetId));
        Assert.DoesNotContain(packetId, publishing.ReleasedPublications);
    }
}
