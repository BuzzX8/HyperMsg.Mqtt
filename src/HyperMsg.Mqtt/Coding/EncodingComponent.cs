using HyperMsg.Buffers;
using HyperMsg.Messaging;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public class EncodingComponent
{
    public void Attach(IBufferingContext bufferingContext)
    {
        ArgumentNullException.ThrowIfNull(bufferingContext);

        //messagingContext.HandlerRegistry.Register<Packet>(Handle);
    }

    public void Detach(IBufferingContext bufferingContext)
    {
        ArgumentNullException.ThrowIfNull(bufferingContext);

        //messagingContext.HandlerRegistry.Unregister<Packet>(Handle);
    }

    public void Handle(Packet packet)
    {
        //Encoding.Encode(_bufferWriter.GetSpan(), packet, out int bytesWritten);
        //_bufferWriter.Advance(bytesWritten);
    }
}
