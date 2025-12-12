using HyperMsg.Buffers;

namespace HyperMsg.Mqtt.Client;

public class PacketListener : IPacketListener
{
    private readonly IBufferingContext bufferingContext;

    public bool IsActive { get; private set; }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    private Task HandleInputBuffer(IBuffer buffer, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public event PacketHandler? PacketAccepted;
}
