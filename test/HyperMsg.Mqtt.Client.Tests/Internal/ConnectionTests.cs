using FakeItEasy;
using HyperMsg.Mqtt.Client.Internal;
using HyperMsg.Mqtt.Packets;
using System.Net;
using Xunit;

namespace HyperMsg.Mqtt.Client.Tests.Internal;

public class ConnectionTests
{
    private readonly IMqttChannel channel;
    private readonly ConnectionSettings settings;
    private readonly Connection connection;

    private readonly EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 0);

    public ConnectionTests()
    {
        channel = A.Fake<IMqttChannel>();
        settings = new(Guid.NewGuid().ToString())
        {
            EndPoint = endPoint
        };
        connection = new(channel, settings);
    }

    [Fact]
    public async Task ConnectAsync_Opens_Channel()
    {
        SetSuccessResponse();

        await connection.ConnectAsync();

        A.CallTo(() => channel.OpenAsync(A<CancellationToken>._)).MustHaveHappened();
    }

    [Fact]
    public async Task ConnectAsync_Sends_Correct_ConnAck_Packet()
    {
        var packet = default(Packet);

        A.CallTo(() => channel.SendAsync(A<Packet>._, A<CancellationToken>._)).Invokes((Packet p, CancellationToken _) => packet = p);
        SetSuccessResponse();

        await connection.ConnectAsync();

        Assert.True(packet.IsConnect);
    }

    private void SetSuccessResponse()
    {
        A.CallTo(() => channel.ReceiveAsync(A<CancellationToken>._)).Returns(new ConnAck(ConnectReasonCode.Success).ToPacket());
    }

    [Fact]
    public async Task ConnectAsync_Throws_Exception_If_Not_ConnAck_Received()
    {
        A.CallTo(() => channel.ReceiveAsync(A<CancellationToken>._)).Returns(new Subscribe(0).ToPacket());

        await Assert.ThrowsAsync<MqttClientException>(() => connection.ConnectAsync());
    }
}
