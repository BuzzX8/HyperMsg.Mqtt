using HyperMsg.Buffers;
using HyperMsg.Transport;

namespace HyperMsg.Mqtt.Client;

public record ClientContext(IBufferingContext bufferingContext, IConnection connection) : IClientContext
{
    public IConnection Connection { get; } = connection;

    public IPacketChannel Channel { get; } = new PacketChannel(bufferingContext);

    public IPacketListener Listener { get; } = new PacketListener(bufferingContext);
}
