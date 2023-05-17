using FakeItEasy;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Socket;
using System.Net;
using Xunit;

namespace HyperMsg.Mqtt.Client.Internal;

public class ConnectionTests
{
    private readonly IMqttChannel channel;
    private readonly ConnectionSettings settings;
    private readonly Connection service;

    private readonly EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 0);

    public ConnectionTests()
    {
        channel = A.Fake<IMqttChannel>();
        settings = new(Guid.NewGuid().ToString())
        {
            EndPoint = endPoint
        };
        service = new(channel, settings);        
    }

    [Fact]
    public void RequestConnection_Dispatches_Connect_Request()
    {
        var dispatchedRequest = default(ConnectRequest);

        service.ConnectAsync();

        Assert.Equal(endPoint, dispatchedRequest.RemoteEndPoint);
    }

    [Fact]
    public void ConnectResult_Dispatches_Connect_Packet()
    {
        var connectPacket = default(Connect);

        Assert.NotNull(connectPacket);
    }

    [Fact]
    public void ConAck_Response_Dispatches_ConnectionResult()
    {
        var connAck = new ConnAck(ConnectReasonCode.Success, true);

        var response = default(ConnectionResponse);

        Assert.NotNull(response);
        Assert.Equal(connAck.ReasonCode, response.ResultCode);
        Assert.Equal(connAck.SessionPresent, response.SessionPresent);
    }
}
