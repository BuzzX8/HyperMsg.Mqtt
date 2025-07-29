using HyperMsg.Buffers;
using HyperMsg.Messaging;

namespace HyperMsg.Mqtt.Coding;

public class DecodingComponent
{
    public void Attach(IBufferingContext bufferingContext)
    {
        throw new NotImplementedException();
    }

    public void Detach(IBufferingContext bufferingContext)
    {
        throw new NotImplementedException();
    }

    public void Handle(Span<byte> data)
    {
        var packet = Decoding.Decode(data, out var bytesRead);
    }
}
