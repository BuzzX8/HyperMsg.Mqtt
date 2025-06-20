using HyperMsg.Buffers;
using HyperMsg.Messaging;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public class EncodingComponent : IMessagingComponent
{
    IBufferWriter _bufferWriter;

    public EncodingComponent(IBufferWriter bufferWriter)
    {
        _bufferWriter = bufferWriter;
    }

    public void Attach(IMessagingContext messagingContext)
    {
        ArgumentNullException.ThrowIfNull(messagingContext);

        messagingContext.HandlerRegistry.Register<Packet>(Handle);
    }

    public void Detach(IMessagingContext messagingContext)
    {
        ArgumentNullException.ThrowIfNull(messagingContext);

        messagingContext.HandlerRegistry.Unregister<Packet>(Handle);
    }

    public void Handle(Packet packet)
    {
        Encoding.Encode(_bufferWriter.GetSpan(), packet, out int bytesWritten);
        _bufferWriter.Advance(bytesWritten);
    }
}
