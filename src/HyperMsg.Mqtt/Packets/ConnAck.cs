namespace HyperMsg.Mqtt.Packets;

/// <summary>
/// Represents an MQTT CONNACK packet, which is sent by the server in response to a CONNECT packet from a client.
/// </summary>
/// <param name="ReasonCode">The reason code indicating the result of the connection attempt.</param>
/// <param name="SessionPresent">Indicates whether the server has a session present for the client.</param>
/// <param name="Properties">The properties associated with the CONNACK packet (MQTT 5.0).</param>
public record ConnAck(
    ConnectReasonCode ReasonCode,
    bool SessionPresent = false,
    ConnAckProperties Properties = null)
{
    /// <summary>
    /// Converts this <see cref="ConnAck"/> instance to a <see cref="Packet"/> of type CONNACK.
    /// </summary>
    /// <returns>A <see cref="Packet"/> representing this CONNACK packet.</returns>
    public Packet ToPacket() => new(PacketType.ConAck, this);

    /// <summary>
    /// Returns a string representation of the CONNACK packet.
    /// </summary>
    /// <returns>A string containing the session present flag and reason code.</returns>
    public override string ToString() => $"ConnAck(SP={SessionPresent},Code={ReasonCode})";

    /// <summary>
    /// Implicitly converts a <see cref="ConnAck"/> instance to a <see cref="Packet"/>.
    /// </summary>
    /// <param name="connAck">The <see cref="ConnAck"/> instance.</param>
    public static implicit operator Packet(ConnAck connAck) => connAck.ToPacket();
}
