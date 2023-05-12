using System.Net;

namespace HyperMsg.Mqtt;

public class ConnectionSettings
{
    public ConnectionSettings(string clientId)
    {
        ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
    }

    public EndPoint EndPoint { get; set; }

    public bool CleanSession { get; set; }

    /// <summary>
    /// Unique ID of client for server.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// Time interval measured in seconds.
    /// </summary>
    public ushort KeepAlive { get; set; }

    public WillMessageSettings WillMessageSettings { get; set; }

    public bool UseTls { get; set; }
}