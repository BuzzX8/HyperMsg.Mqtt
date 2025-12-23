using HyperMsg.Buffers;
using HyperMsg.Mqtt.Coding;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Client;

internal class PacketChannel(IBufferingContext bufferingContext) : IPacketChannel
{
    public async ValueTask<Packet> ReceiveAsync(CancellationToken cancellationToken)
    {
        await bufferingContext.RequestInputBufferUpstreamUpdate(cancellationToken);

        var reader = bufferingContext.InputBuffer.Reader;
        var buffer = reader.GetMemory();
        var (packet, bytesDecoded) = Decoding.Decode(buffer);

        reader.Advance((int)bytesDecoded);

        return packet;
    }

    public async ValueTask SendAsync(Packet packet, CancellationToken cancellationToken)
    {
        var writer = bufferingContext.OutputBuffer.Writer;
        var buffer = writer.GetMemory();
        var bytesEncoded = Encoding.Encode(buffer, packet);
        
        writer.Advance((int)bytesEncoded);

        await bufferingContext.RequestOutputBufferDownstreamUpdate(cancellationToken);
    }
}
