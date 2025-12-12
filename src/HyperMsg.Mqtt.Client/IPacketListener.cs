using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

public interface IPacketListener
{
    bool IsActive { get; }

    void Start();

    void Stop();

    event PacketHandler? PacketAccepted;
}

public delegate Task PacketHandler(Packet packet, CancellationToken cancellationToken);