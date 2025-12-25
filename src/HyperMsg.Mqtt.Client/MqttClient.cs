using HyperMsg.Mqtt.Client.Components;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

/// <summary>
/// High-level MQTT client that coordinates connection, publishing and subscription components.
/// </summary>
public class MqttClient
{
    /// <summary>
    /// The client context providing the underlying channel and runtime information.
    /// </summary>
    private readonly IClientContext clientContext;

    /// <summary>
    /// Component responsible for establishing and managing the MQTT connection.
    /// </summary>
    private readonly Connection connection;

    /// <summary>
    /// Component responsible for publishing messages.
    /// </summary>
    private readonly Publishing publishing;

    /// <summary>
    /// Component responsible for subscription management.
    /// </summary>
    private readonly Subscription subscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttClient"/> class.
    /// </summary>
    /// <param name="clientContext">Client context that provides the communication channel.</param>
    /// <param name="settings">Connection settings to use when connecting to the broker.</param>
    public MqttClient(IClientContext clientContext, ConnectionSettings settings)
    {
        this.clientContext = clientContext;

        connection = new(clientContext.Channel, settings);
        publishing = new(SendPacketAsync);
        subscription = new(SendPacketAsync);

        this.clientContext.Listener.PacketAccepted += HandleAcceptedPacket;
    }

    private async Task HandleAcceptedPacket(Packet packet, CancellationToken cancellationToken)
    {
        switch (packet.Kind)
        {
            case PacketKind.Publish:
                var publishPacket = packet.ToPublish();
                PublishReceived?.Invoke(publishPacket);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Connects to the configured MQTT broker.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the connect operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the connection is established.</returns>
    public Task ConnectAsync(CancellationToken cancellationToken = default) => connection.ConnectAsync(cancellationToken);

    /// <summary>
    /// Disconnects from the MQTT broker.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the disconnect operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the client has disconnected.</returns>
    public Task DisconnectAsync(CancellationToken cancellationToken = default) => connection.DisconnectAsync(cancellationToken);

    /// <summary>
    /// Sends a PINGREQ to the broker to keep the connection alive.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the ping operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the ping round-trip has finished.</returns>
    public Task PingAsync(CancellationToken cancellationToken = default) => connection.PingAsync(cancellationToken);

    /// <summary>
    /// Requests (subscribes) to the specified topic filters with given subscription options.
    /// </summary>
    /// <param name="requests">Enumerable of subscription requests describing topic filters and options.</param>
    /// <param name="cancellationToken">Token to cancel the subscription request.</param>
    /// <returns>A <see cref="Task"/> that completes when the subscription handshake finishes.</returns>
    public Task RequestSubscriptionAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default) => subscription.RequestSubscriptionAsync(requests, cancellationToken);

    /// <summary>
    /// Unsubscribes from the specified topic filters.
    /// </summary>
    /// <param name="topicfilters">The topic filters to unsubscribe from.</param>
    /// <param name="cancellationToken">Token to cancel the unsubscription operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the unsubscription handshake finishes.</returns>
    public Task UnsubscribeAsync(IEnumerable<string> topicfilters, CancellationToken cancellationToken = default) => subscription.RequestUnsubscriptionAsync(topicfilters, cancellationToken);

    /// <summary>
    /// Sends a raw MQTT <see cref="Packet"/> over the client's channel.
    /// </summary>
    /// <param name="packet">The packet to send.</param>
    /// <param name="cancellationToken">Token to cancel the send operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
    /// <exception cref="NotImplementedException">Thrown until the packet send implementation is provided.</exception>
    private Task SendPacketAsync(Packet packet, CancellationToken cancellationToken = default) => clientContext.Channel.SendAsync(packet, cancellationToken).AsTask();

    /// <summary>
    /// Publishes a message to the specified topic.
    /// </summary>
    /// <param name="topicName">The topic name to publish to.</param>
    /// <param name="message">The message payload as a <see cref="ReadOnlyMemory{Byte}"/>.</param>
    /// <param name="qos">Quality of Service level for the publish (default: <see cref="QosLevel.Qos0"/>).</param>
    /// <param name="retainMessage">Whether the broker should retain the message (default: <c>false</c>).</param>
    /// <param name="cancellationToken">Token to cancel the publish operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the publish workflow completes according to the chosen QoS.</returns>
    public Task PublishAsync(string topicName, ReadOnlyMemory<byte> message, QosLevel qos = QosLevel.Qos0, bool retainMessage = false, CancellationToken cancellationToken = default) 
        => publishing.PublishAsync(new PublishRequest(topicName, message, qos, retainMessage), cancellationToken);

    /// <summary>
    /// Occurs when a PUBLISH packet is received from the broker.
    /// </summary>
    /// <remarks>
    /// Subscribers can attach handlers to this event to process incoming application messages.
    /// The event receives the parsed <see cref="Packets.Publish"/> packet.
    /// </remarks>
    public event Action<Packets.Publish> PublishReceived;
}
