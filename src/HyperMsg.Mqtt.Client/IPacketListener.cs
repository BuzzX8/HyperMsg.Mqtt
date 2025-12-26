using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

/// <summary>
/// Represents a component that listens for incoming MQTT packets and notifies subscribers
/// when packets are accepted for processing.
/// Implementations control the listener lifecycle via <see cref="Start"/> and <see cref="Stop"/>.
/// </summary>
public interface IPacketListener
{
    /// <summary>
    /// Gets a value indicating whether the listener is currently active and accepting packets.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Starts the listener so it begins receiving and accepting incoming packets.
    /// Implementations should begin any required background processing and resource allocation.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the listener and halts processing of incoming packets.
    /// Implementations should cancel or complete outstanding work and release resources as appropriate.
    /// </summary>
    void Stop();

    /// <summary>
    /// Occurs when a packet has been accepted by the listener and is ready for processing.
    /// Handlers are invoked with the accepted <see cref="Packet"/> and a <see cref="CancellationToken"/>
    /// that can be used to observe shutdown or cancellation requests.
    /// </summary>
    event PacketHandler? PacketAccepted;
}

/// <summary>
/// Delegate invoked when a packet is accepted by an <see cref="IPacketListener"/>.
/// </summary>
/// <param name="packet">The accepted MQTT <see cref="Packet"/> to process.</param>
/// <param name="cancellationToken">
/// A token that should be observed for cooperative cancellation (for example when the listener is stopping).
/// </param>
/// <returns>A <see cref="ValueTask"/> that completes when the handler has finished processing the packet.</returns>
public delegate ValueTask PacketHandler(Packet packet, CancellationToken cancellationToken);