namespace HyperMsg.Mqtt.Packets;

/// <summary>
/// Flags that represent the MQTT CONNECT control flags found in the Connect packet's variable header.
/// These flags indicate session behavior, Will message options, QoS for the Will message, and presence of
/// authentication fields (username/password).
/// </summary>
/// <remarks>
/// This enum is a bit field; multiple values may be combined using bitwise OR. The QoS level for the Will
/// message is represented by the Qos0/Qos1/Qos2 values (mutually exclusive bits). The underlying storage
/// is a single byte, matching the MQTT protocol connect flags byte layout.
/// </remarks>
[Flags]
public enum ConnectFlags : byte
{
    /// <summary>
    /// No flags are set.
    /// </summary>
    None = 0,

    /// <summary>
    /// If set, the server should discard any previous session and start a clean session for the client.
    /// </summary>
    CleanSession = 0x02,

    /// <summary>
    /// Indicates that a Will message is included in the CONNECT packet.
    /// </summary>
    Will = 0x04,

    /// <summary>
    /// Will QoS 0 (at most once). Represents no delivery guarantee for the Will message.
    /// This value is the zero value for the QoS bits.
    /// </summary>
    Qos0 = 0x00,

    /// <summary>
    /// Will QoS 1 (at least once). Represents the Will message should be delivered at least once.
    /// </summary>
    Qos1 = 0x08,

    /// <summary>
    /// Will QoS 2 (exactly once). Represents the Will message should be delivered exactly once.
    /// </summary>
    Qos2 = 0x10,

    /// <summary>
    /// If set, the Will message should be retained by the server.
    /// </summary>
    WillRetain = 0x20,

    /// <summary>
    /// Indicates that a password is present in the CONNECT payload.
    /// </summary>
    Password = 0x40,

    /// <summary>
    /// Indicates that a username is present in the CONNECT payload.
    /// </summary>
    UserName = 0x80
}
