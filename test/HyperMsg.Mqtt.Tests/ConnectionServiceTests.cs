using HyperMsg.Mqtt.Packets;
using HyperMsg.Socket;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace HyperMsg.Mqtt;

public class ConnectionServiceTests
{
    private readonly MessageBroker broker;
    private readonly ConnectionSettings settings;
    private readonly ConnectionService service;

    private readonly EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 0);

    public ConnectionServiceTests()
    {
        broker = new();
        settings = new(Guid.NewGuid().ToString())
        {
            EndPoint = endPoint
        };
        service = new(broker, settings);
        service.StartAsync(default);
    }

    [Fact]
    public void RequestConnection_Dispatches_Connect_Request()
    {
        var dispatchedRequest = default(ConnectRequest);
        broker.Register<ConnectRequest>(r => dispatchedRequest = r);

        service.RequestConnection();

        Assert.Equal(endPoint, dispatchedRequest.RemoteEndPoint);
    }

    [Fact]
    public void ConnectResult_Dispatches_Connect_Packet()
    {
        var connectPacket = default(Connect);
        broker.Register<Connect>(c => connectPacket = c);

        broker.Dispatch(new ConnectResult(endPoint, SocketError.Success));

        Assert.NotNull(connectPacket);
    }

    [Fact]
    public void ConAck_Response_Dispatches_ConnectionResult()
    {
        var connAck = new ConnAck(ConnectReasonCode.Success, true);

        var response = default(ConnectionResponse);
        broker.Register<ConnectionResponse>(r => response = r);

        broker.Dispatch(connAck);

        Assert.NotNull(response);
        Assert.Equal(connAck.ReasonCode, response.ResultCode);
        Assert.Equal(connAck.SessionPresent, response.SessionPresent);
    }
}
