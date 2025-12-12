using HyperMsg.Buffers;
using HyperMsg.Transport;

namespace HyperMsg.Mqtt.Client;

public class ClientContext : IClientContext
{
    private readonly IBufferingContext bufferingContext;

    public ClientContext(IBufferingContext bufferingContext, IConnection connection)
    {
        this.bufferingContext = bufferingContext;
        Connection = connection;
    }

    public IConnection Connection { get; }

    public IPacketChannel Channel => throw new NotImplementedException();

    public IPacketListener Listener => throw new NotImplementedException();
}
