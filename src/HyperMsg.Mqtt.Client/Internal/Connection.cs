using HyperMsg.Mqtt.Packets;
using HyperMsg.Socket;

namespace HyperMsg.Mqtt.Client.Internal;

public class Connection
{
    private readonly IMqttChannel channel;
    private readonly ConnectionSettings settings;

    public Connection(IMqttChannel channel, ConnectionSettings settings)
    {
        this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
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

    private void HandleConAck(ConnAck response)
    {

    }
}

public record ConnectionResponse(ConnectReasonCode ResultCode, bool SessionPresent);
