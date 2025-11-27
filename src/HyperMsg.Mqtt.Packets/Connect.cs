namespace HyperMsg.Mqtt.Packets;

/// <summary>
/// Represents an MQTT CONNECT packet, used by a client to establish a connection to an MQTT broker.
/// </summary>
public record Connect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Connect"/> record.
    /// </summary>
    private Connect(byte version, string clientId) 
    {
        ProtocolName = "MQTT";
        ProtocolVersion = version;
        ClientId = clientId;
    }

    #region Variable header

    /// <summary>
    /// Gets or sets the protocol name (e.g., "MQTT").
    /// </summary>
    public string ProtocolName { get; }

    /// <summary>
    /// Gets or sets the protocol version (e.g., 5 for MQTT 5.0).
    /// </summary>
    public byte ProtocolVersion { get; }

    /// <summary>
    /// Gets or sets the connect flags, indicating session and authentication options.
    /// </summary>
    public ConnectFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the keep alive time in seconds.
    /// </summary>
    public ushort KeepAlive { get; set; }

    /// <summary>
    /// Gets or sets the properties for the CONNECT packet (MQTT 5.0).
    /// </summary>
    public ConnectProperties Properties { get; set; }

    #endregion

    #region Payload

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the properties for the Will message (MQTT 5.0).
    /// </summary>
    public ConnectWillProperties WillProperties { get; set; }

    /// <summary>
    /// Gets or sets the Will topic.
    /// </summary>
    public string WillTopic { get; set; }

    /// <summary>
    /// Gets or sets the Will payload.
    /// </summary>
    public ReadOnlyMemory<byte> WillPayload { get; set; }

    /// <summary>
    /// Gets or sets the user name for authentication.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    public ReadOnlyMemory<byte> Password { get; set; }

    #endregion

    /// <summary>
    /// Creates a new MQTT 5.0 <see cref="Connect"/> packet with the specified client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A new <see cref="Connect"/> instance for MQTT 5.0.</returns>
    public static Connect NewV5(string clientId) => new(5, clientId);

    /// <summary>
    /// Implicitly converts a <see cref="Connect"/> instance to a <see cref="Packet"/>.
    /// </summary>
    /// <param name="connect">The <see cref="Connect"/> instance.</param>
    public static implicit operator Packet(Connect connect) => Packet.From(connect);
}
