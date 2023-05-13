using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client.Internal;

public interface IMqttChannel
{
    Task OpenAsync(CancellationToken cancellationToken);

    Task CloseAsync(CancellationToken cancellationToken);

    Task<int> SendAsync(Packet packet, CancellationToken cancellationToken);

    Task<Packet> ReceiveAsync(CancellationToken cancellationToken);
}
