namespace HyperMsg.Mqtt;

public class WillMessageSettings
{
    public WillMessageSettings(string topic, ReadOnlyMemory<byte> message, bool retain)
    {
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        Message = message;
        Retain = retain;
    }

    public string Topic { get; }

    public ReadOnlyMemory<byte> Message { get; }

    public bool Retain { get; }
}
