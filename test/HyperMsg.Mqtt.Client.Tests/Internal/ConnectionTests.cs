using FakeItEasy;
using HyperMsg.Mqtt.Packets;
using System.Net;
using Xunit;

namespace HyperMsg.Mqtt.Client.Internal;

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
        A.CallTo(() => channel.ReceiveAsync(A<CancellationToken>._)).Returns(new ConnAck(ConnectReasonCode.Success).ToPacket());

        await connection.ConnectAsync();

        A.CallTo(() => channel.OpenAsync(A<CancellationToken>._)).MustHaveHappened();
    }

    [Fact]
    public async Task ConnectAsync_Sends_Correct_ConnAck_Packet()
    {
        var packet = default(Packet);

        A.CallTo(() => channel.SendAsync(A<Packet>._, A<CancellationToken>._)).Invokes((Packet p, CancellationToken _) => packet = p);
        A.CallTo(() => channel.ReceiveAsync(A<CancellationToken>._)).Returns(new ConnAck(ConnectReasonCode.Success).ToPacket());

        await connection.ConnectAsync();

        Assert.True(packet.IsConnect);
    }
}
