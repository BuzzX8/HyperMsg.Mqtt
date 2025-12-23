using HyperMsg.Transport;

namespace HyperMsg.Mqtt.Client;

public interface IClientContext
{
    IConnection Connection { get; }

    IPacketChannel Channel { get; }

    IPacketListener Listener { get; }
}
