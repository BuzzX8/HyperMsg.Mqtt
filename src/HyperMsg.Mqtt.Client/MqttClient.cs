using HyperMsg.Mqtt.Client.Internal;

namespace HyperMsg.Mqtt.Client;

public class MqttClient
{
    private readonly Connection connection;

    public MqttClient(IMqttChannel channel, ConnectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(channel, nameof(channel));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        connection = new(channel, settings);
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default) => connection.ConnectAsync(cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken = default) => connection.DisconnectAsync(cancellationToken);

    public Task PingAsync(CancellationToken cancellationToken = default) => connection.PingAsync(cancellationToken);

    public Task SubscribeAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task UnsubscribeAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task PublishAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public event Action<Packets.Publish> PublishReceived;
}
