using HyperMsg.Buffers;
using HyperMsg.Mqtt.Coding;

namespace HyperMsg.Mqtt.Client;

internal class PacketListener(IBufferingContext bufferingContext) : IPacketListener, IDisposable
{
    public bool IsActive { get; private set; }

    public void Start()
    {
        if (IsActive)
        {
            return;
        }

        bufferingContext.InputBufferDownstreamUpdateRequested += HandleInputBuffer;
        IsActive = true;
    }

    public void Stop()
    {
        if (!IsActive)
        {
            return;
        }

        bufferingContext.InputBufferDownstreamUpdateRequested -= HandleInputBuffer;
        IsActive = false;
    }

    private ValueTask HandleInputBuffer(IBuffer buffer, CancellationToken cancellationToken)
    {
        var reader = buffer.Reader;
        var memory = reader.GetMemory();
        (var packet, var bytesDecoded) = Decoding.Decode(memory);

        reader.Advance((int)bytesDecoded);

        return PacketAccepted?.Invoke(packet, cancellationToken) ?? ValueTask.CompletedTask;
    }
    
    public event PacketHandler? PacketAccepted;

    public void Dispose()
    {
        Stop();
    }
}
