using HyperMsg.Mqtt.Packets;

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
        await channel.OpenAsync(cancellationToken);

        var connect = CreateConnectPacket(settings);

        await channel.SendAsync(connect, cancellationToken);
        var response = await channel.ReceiveAsync(cancellationToken);

        if (!response.IsConnAck)
        {
            throw new MqttClientException($"Protocol error. Expected to receive ConnAck but received {response.Type}");
        }

        var connAck = response.ToConnAck();

        if (connAck.ReasonCode != ConnectReasonCode.Success)
        {
            await channel.CloseAsync(default);
            throw new MqttClientException($"{connAck.ReasonCode}");
        }
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

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task PingAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public record ConnectionResponse(ConnectReasonCode ResultCode, bool SessionPresent);
