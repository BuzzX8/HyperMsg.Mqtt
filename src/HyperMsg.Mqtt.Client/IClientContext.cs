using HyperMsg.Transport;

namespace HyperMsg.Mqtt.Client;

/// <summary>
/// Provides access to the underlying client resources required for MQTT operations:
/// the transport connection, the packet channel used for sending/receiving packets,
/// and the packet listener for incoming packet notifications.
/// </summary>
public interface IClientContext
{
    /// <summary>
    /// Gets the transport connection that carries MQTT packets.
    /// Implementations expose the underlying <see cref="IConnection"/> used by the client.
    /// </summary>
    IConnection Connection { get; }

    /// <summary>
    /// Gets the packet channel used to asynchronously send and receive MQTT <see cref="HyperMsg.Mqtt.Packets.Packet"/> instances.
    /// </summary>
    IPacketChannel Channel { get; }

    /// <summary>
    /// Gets the packet listener responsible for accepting incoming packets and raising notifications.
    /// </summary>
    IPacketListener Listener { get; }
}
