using HyperMsg.Mqtt.Packets;
using System;

namespace HyperMsg.Mqtt;

public class ConnectionService : IDisposable
{
    private readonly IDispatcher dispatcher;
    private readonly IRegistry registry;

    private readonly ConnectionSettings connectionSettings;

    public ConnectionService(IDispatcher dispatcher, IRegistry registry, ConnectionSettings connectionSettings)
    {
        this.dispatcher = dispatcher;
        this.registry = registry;
        this.connectionSettings = connectionSettings;

        registry.Register<ConnAck>(HandleConAck);
    }

    public void RequestConnection()
    {
        var connectPacket = CreateConnectPacket(connectionSettings);
        dispatcher.Dispatch(connectPacket);
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

    public void Dispose() => registry.Deregister<ConnAck>(HandleConAck);
}
