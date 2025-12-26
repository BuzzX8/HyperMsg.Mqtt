using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

/// <summary>
/// Represents an asynchronous channel used to send and receive MQTT <see cref="Packet"/> instances.
/// Implementations provide the transport semantics for writing outgoing packets and reading incoming packets,
/// and must honor the provided <see cref="CancellationToken"/> for cooperative cancellation.
/// </summary>
public interface IPacketChannel
{
    /// <summary>
    /// Sends the specified <paramref name="packet"/> over the channel.
    /// Implementations should complete the returned <see cref="ValueTask"/> when the packet has been
    /// successfully handed off to the transport or queued for transmission.
    /// </summary>
    /// <param name="packet">The MQTT <see cref="Packet"/> to send.</param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the send operation to complete. If cancellation is requested,
    /// the operation should abort promptly and throw an appropriate cancellation exception.
    /// </param>
    ValueTask SendAsync(Packet packet, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously receives the next <see cref="Packet"/> from the channel.
    /// The returned <see cref="Packet"/> represents the next available incoming MQTT packet.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for an incoming packet. If cancellation is requested,
    /// the operation should abort promptly and throw an appropriate cancellation exception.
    /// </param>
    /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to the received <see cref="Packet"/>.</returns>
    ValueTask<Packet> ReceiveAsync(CancellationToken cancellationToken);
}
