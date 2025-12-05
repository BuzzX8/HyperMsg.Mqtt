namespace HyperMsg.Mqtt.Packets;

/// <summary>
/// Represents the set of MQTT CONNECT properties that can be carried in a CONNECT packet.
/// These properties convey client preferences and authentication information used by the server
/// when establishing a session.
/// </summary>
public class ConnectProperties
{
    /// <summary>
    /// Session Expiry Interval in seconds. A value of 0 indicates the session ends when the network
    /// connection is closed. If not present, the server's default applies.
    /// </summary>
    public uint SessionExpiryInterval { get; set; }

    /// <summary>
    /// The maximum number of QoS 1 and QoS 2 publications that the client is willing to process
    /// concurrently. The server should not send more publications than this value.
    /// </summary>
    public ushort ReceiveMaximum { get; set; }

    /// <summary>
    /// The maximum packet size in bytes that the client is willing to accept. Values larger than
    /// this SHOULD NOT be sent to the client.
    /// </summary>
    public uint MaximumPacketSize { get; set; }

    /// <summary>
    /// The maximum number of topic aliases the client supports. Topic aliases allow replacing topic
    /// strings with integer aliases to reduce packet size.
    /// </summary>
    public ushort TopicAliasMaximum { get; set; }

    /// <summary>
    /// If true, the client requests that the server include response information in responses
    /// where applicable.
    /// </summary>
    public bool RequestResponseInformation { get; set; }

    /// <summary>
    /// If true, the client requests that the server include problem information in error responses.
    /// </summary>
    public bool RequestProblemInformation { get; set; }

    /// <summary>
    /// The name of the authentication method the client wishes to use. This is optional and may be null.
    /// </summary>
    public string? AuthenticationMethod { get; set; }

    /// <summary>
    /// Authentication data associated with <see cref="AuthenticationMethod"/>. Binary blob; optional.
    /// </summary>
    public ReadOnlyMemory<byte>? AuthenticationData { get; set; }

    /// <summary>
    /// User-defined properties expressed as key/value pairs. May be null or empty when no user properties are present.
    /// </summary>
    public IDictionary<string, string>? UserProperties { get; set; }
}
