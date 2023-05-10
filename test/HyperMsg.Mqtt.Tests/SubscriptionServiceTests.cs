using HyperMsg.Mqtt.Packets;
using Xunit;

namespace HyperMsg.Mqtt;

public class SubscriptionServiceTests
{
    private readonly MessageBroker messageBroker;
    private readonly SubscriptionService service;

    public SubscriptionServiceTests()
    {
        messageBroker = new();
        service = new(messageBroker);
        service.StartAsync(default);
    }

    [Fact]
    public void RequestSubscription_Sends_Correct_Subscribe_Request()
    {
        //var subscribePacket = default(Subscribe);
        //messageBroker.Register<Subscribe>(subscribe => subscribePacket = subscribe);
        //var request = Enumerable.Range(1, 5)
        //    .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
        //    .ToArray();

        //var packetId = service.RequestSubscription(request);
        //Assert.Equal(packetId, subscribePacket.Id);
        //Assert.True(service.PendingSubscriptionRequests.ContainsKey(packetId));
    }

    [Fact]
    public void SubAck_Response_Updates_PendingSubscriptionRequests()
    {
        //var request = Enumerable.Range(1, 5)
        //    .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
        //    .ToArray();

        //var packetId = service.RequestSubscription(request);
        //var subAck = new SubAck(packetId, new[] { SubscriptionResult.Failure, SubscriptionResult.SuccessQos1, SubscriptionResult.SuccessQos0 });
        //messageBroker.Dispatch(subAck);

        //Assert.False(service.PendingSubscriptionRequests.ContainsKey(packetId));
    }

    [Fact]
    public void RequestUnsubscription_Sends_Unsubscription_Request()
    {
        var unsubscribe = default(Unsubscribe);
        messageBroker.Register<Unsubscribe>(packet => unsubscribe = packet);
        var topics = new[] { "topic-1", "topic-2" };

        var packetId = service.RequestUnsubscription(topics);

        Assert.Equal(packetId, unsubscribe.Id);
        Assert.Equal(topics, unsubscribe.Topics);
    }
}
