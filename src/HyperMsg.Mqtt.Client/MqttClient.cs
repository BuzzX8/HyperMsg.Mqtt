using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

public class MqttClient
{
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task PingAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task SubscribeAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task UnsubscribeAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public Task PublishAsync(CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

    public event Action<Publish> PublishReceived;
}
