using HyperMsg.Mqtt.Client.Components;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

public class MqttClient
{
    private readonly IClientContext clientContext;
    private readonly Connection connection;
    private readonly Publishing publishing;
    private readonly Subscription subscription;

    public MqttClient(IClientContext clientContext, ConnectionSettings settings)
    {
        this.clientContext = clientContext;
        connection = new(clientContext.Channel, settings);
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default) => connection.ConnectAsync(cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken = default) => connection.DisconnectAsync(cancellationToken);

    public Task PingAsync(CancellationToken cancellationToken = default) => connection.PingAsync(cancellationToken);

    public Task RequestSubscriptionAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default) => subscription.RequestSubscriptionAsync(requests, cancellationToken);

    public Task UnsubscribeAsync(IEnumerable<string> topicfilters, CancellationToken cancellationToken = default) => subscription.RequestUnsubscriptionAsync(topicfilters, cancellationToken);

    public Task PublishAsync(string topicName, ReadOnlyMemory<byte> message, QosLevel qos = QosLevel.Qos0, bool retainMessage = false, CancellationToken cancellationToken = default) 
        => publishing.PublishAsync(new PublishRequest(topicName, message, qos, retainMessage), cancellationToken);

    public event Action<Packets.Publish> PublishReceived;
}
