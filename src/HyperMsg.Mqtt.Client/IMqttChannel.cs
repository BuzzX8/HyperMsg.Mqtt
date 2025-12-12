using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

public interface IMqttChannel
{
    Task OpenAsync(CancellationToken cancellationToken);

    Task CloseAsync(CancellationToken cancellationToken);

    Task SendAsync(Packet packet, CancellationToken cancellationToken);

    Task<Packet> ReceiveAsync(CancellationToken cancellationToken);
}
