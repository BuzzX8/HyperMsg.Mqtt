namespace HyperMsg.Mqtt;

/// <summary>
/// Specifies the MQTT Quality of Service (QoS) levels for message delivery.
/// </summary>
public enum QosLevel : byte
{
    /// <summary>
    /// No QoS level specified.
    /// </summary>
    None = byte.MaxValue,

    /// <summary>
    /// At most once delivery (fire and forget). The message is delivered according to the best efforts of the operating environment. Message loss can occur.
    /// </summary>
    Qos0 = 0,

    /// <summary>
    /// At least once delivery. The message is assured to arrive but duplicates can occur.
    /// </summary>
    Qos1,

    /// <summary>
    /// Exactly once delivery. The message is assured to arrive exactly once.
    /// </summary>
    Qos2
}
