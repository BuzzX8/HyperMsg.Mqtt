using HyperMsg.Mqtt.Packets;
using System.Linq;
using Xunit;

namespace HyperMsg.Mqtt;

public class SubscriptionServiceTests
{
    private readonly MessageBroker messageBroker;
    private readonly SubscriptionService session;

    public SubscriptionServiceTests()
    {
        messageBroker = new();
        session= new(messageBroker, messageBroker);
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

    [Fact]
    public void SubAck_Response_Invokes_Handler_Registered_With_RegisterSubscriptionResponseHandler()
    {
        var actualResult = default(SubscriptionResponseHandlerArgs);

        var request = Enumerable.Range(1, 5)
            .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
            .ToArray();

        messageBroker.Register<SubscriptionResponseHandlerArgs>(response => actualResult = response);
        var packetId = session.RequestSubscription(request);
        var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });
        messageBroker.Dispatch(subAck);

        Assert.NotNull(actualResult);
        //Assert.False(requestStorage.Contains<Subscribe>(packetId));
        Assert.Equal(subAck.Results, actualResult.SubscriptionResults);
    }

    [Fact]
    public void RequestUnsubscription_Sends_Unsubscription_Request()
    {
        var unsubscribe = default(Unsubscribe);
        messageBroker.Register<Unsubscribe>(packet => unsubscribe = packet);
        var topics = new[] { "topic-1", "topic-2" };

        var packetId = messageBroker.SendUnsubscribeRequest(topics);

        Assert.NotNull(unsubscribe);
        Assert.Equal(packetId, unsubscribe.Id);
        Assert.Equal(topics, unsubscribe.Topics);
        //Assert.True(requestStorage.Contains<Unsubscribe>(unsubscribe.Id));
    }
}
