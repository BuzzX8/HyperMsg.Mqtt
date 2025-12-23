using FakeItEasy;
using HyperMsg.Buffers;
using HyperMsg.Transport;
using Xunit;

namespace HyperMsg.Mqtt.Client.Tests;

public class ClientContextTests
{
    private readonly ClientContext clientContext;
    private readonly IConnection connection;

    public ClientContextTests()
    {
        var bufferingContext = new BufferingContext();
        connection = A.Fake<IConnection>();

        clientContext = new ClientContext(bufferingContext, connection);
    }

    [Fact]
    public void Connection_property_returns_supplied_instance()
    {
        Assert.Same(connection, clientContext.Connection);
    }

    [Fact]
    public void Channel_and_Listener_are_initialized_and_implement_interfaces()
    {
        Assert.NotNull(clientContext.Channel);
        Assert.IsType<IPacketChannel>(clientContext.Channel, exactMatch: false);

        Assert.NotNull(clientContext.Listener);
        Assert.IsType<IPacketListener>(clientContext.Listener, exactMatch: false);
    }
}
