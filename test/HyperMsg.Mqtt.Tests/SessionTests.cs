using HyperMsg.Mqtt.Packets;
using System.Linq;
using Xunit;

namespace HyperMsg.Mqtt;

public class SessionTests
{
    private readonly MessageBroker messageBroker;
    private readonly Session session;

    public SessionTests()
    {
        messageBroker = new();
        session= new(messageBroker);
    }

    [Fact]
    public void RequestSubscription_Sends_Correct_Subscribe_Request()
    {
        var subscribePacket = default(Subscribe);
        messageBroker.Register<Subscribe>(subscribe => subscribePacket = subscribe);
        var request = Enumerable.Range(1, 5)
            .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
            .ToArray();

        var packetId = session.RequestSubscription(request);

        Assert.NotNull(subscribePacket);
        Assert.Equal(packetId, subscribePacket.Id);
        //Assert.True(requestStorage.Contains<Subscribe>(packetId));
    }
}
