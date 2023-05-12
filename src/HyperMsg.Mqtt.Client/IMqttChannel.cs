namespace HyperMsg.Mqtt.Client;

public interface IMqttChannel
{
    Task OpenAsync(CancellationToken cancellationToken);

    Task CloseAsync(CancellationToken cancellationToken);

    Task<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

    Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
}
