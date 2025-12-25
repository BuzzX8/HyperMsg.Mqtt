using HyperMsg.Mqtt.Client.Components;

namespace HyperMsg.Mqtt.Client;

public class MqttClient
{
    private readonly IClientContext clientContext;
    private readonly Connection connection;
    private readonly Publishing publishing;

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

    public Task PublishAsync(string topicName, ReadOnlyMemory<byte> message, QosLevel qos = QosLevel.Qos0, bool retainMessage = false, CancellationToken cancellationToken = default) 
        => publishing.PublishAsync(new PublishRequest(topicName, message, qos, retainMessage), cancellationToken);

    public event Action<Packets.Publish> PublishReceived;
}
