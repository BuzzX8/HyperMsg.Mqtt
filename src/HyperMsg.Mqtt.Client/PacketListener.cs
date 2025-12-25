using HyperMsg.Buffers;

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
        throw new NotImplementedException();
    }
    
    public event PacketHandler? PacketAccepted;

    public void Dispose()
    {
        Stop();
    }
}
