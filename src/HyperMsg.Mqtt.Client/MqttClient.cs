using HyperMsg.Mqtt.Client.Components;

namespace HyperMsg.Mqtt.Client;

public class MqttClient
{
    private readonly IClientContext clientContext;
    private readonly Connection connection;

    public MqttClient(IClientContext clientContext, ConnectionSettings settings)
    {
        this.clientContext = clientContext;
        connection = new(clientContext.Channel, settings);
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default) => connection.ConnectAsync(cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken = default) => connection.DisconnectAsync(cancellationToken);

    public Task PingAsync(CancellationToken cancellationToken = default) => connection.PingAsync(cancellationToken);

    public Task SubscribeAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task UnsubscribeAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task PublishAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public event Action<Packets.Publish> PublishReceived;
}
