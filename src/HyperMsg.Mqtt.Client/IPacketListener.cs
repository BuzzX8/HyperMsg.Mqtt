using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

public interface IPacketListener
{
    bool IsActive { get; }

    void Start();

    void Stop();

    event PacketHandler? PacketAccepted;
}

public delegate ValueTask PacketHandler(Packet packet, CancellationToken cancellationToken);