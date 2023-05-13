﻿using HyperMsg.Mqtt.Packets;
using HyperMsg.Socket;

namespace HyperMsg.Mqtt.Client.Internal;

public class ConnectionService : Service
{
    private readonly ConnectionSettings settings;

    public ConnectionService(ITopic messageTopic, ConnectionSettings settings) : base(messageTopic)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void RequestConnection()
    {
        Dispatch(new ConnectRequest(settings.EndPoint));
    }

    private void HandleConnectResult(ConnectResult result)
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
            connect.WillPayload = connectionSettings.WillMessageSettings.Message;
        }

        return connect;
    }

    private void HandleConAck(ConnAck response) => Dispatch(new ConnectionResponse(response.ReasonCode, response.SessionPresent));

    protected override void RegisterHandlers(IRegistry registry)
    {
        registry.Register<ConnectResult>(HandleConnectResult);
        registry.Register<ConnAck>(HandleConAck);
    }

    protected override void UnregisterHandlers(IRegistry registry)
    {
        registry.Unregister<ConnectResult>(HandleConnectResult);
        registry.Unregister<ConnAck>(HandleConAck);
    }
}

public record ConnectionResponse(ConnectReasonCode ResultCode, bool SessionPresent);