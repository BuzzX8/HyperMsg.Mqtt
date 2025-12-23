using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

public interface IPacketChannel
{
    ValueTask SendAsync(Packet packet, CancellationToken cancellationToken);

    ValueTask<Packet> ReceiveAsync(CancellationToken cancellationToken);
}
