using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt;

public class ConnectionService : Service
{
    private readonly ConnectionSettings settings;

    public ConnectionService(ITopic messageTopic, ConnectionSettings settings) : base(messageTopic)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void RequestConnection()
    {
        var connectPacket = CreateConnectPacket(settings);
        Dispatch(connectPacket);
    }

    private static Connect CreateConnectPacket(ConnectionSettings connectionSettings)
    {
        var flags = ConnectFlags.None;

        if (connectionSettings.CleanSession)
        {
            flags |= ConnectFlags.CleanSession;
        }

        var connect = new Connect
        {
            ClientId = connectionSettings.ClientId,
            KeepAlive = connectionSettings.KeepAlive,
            Flags = flags
        };

        if (connectionSettings.WillMessageSettings != null)
        {
            connect.Flags |= ConnectFlags.Will;
            connect.WillTopic = connectionSettings.WillMessageSettings.Topic;
            connect.WillMessage = connectionSettings.WillMessageSettings.Message;
        }

        return connect;
    }

    private void HandleConAck(ConnAck response)
    {

    }

    protected override void RegisterHandlers(IRegistry registry)
    {
        registry.Register<ConnAck>(HandleConAck);
    }

    protected override void UnregisterHandlers(IRegistry registry)
    {
        registry.Unregister<ConnAck>(HandleConAck);
    }
}
