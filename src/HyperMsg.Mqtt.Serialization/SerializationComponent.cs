using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Serialization
{
    internal class SerializationComponent
    {
        private readonly IBuffer buffer;

        internal SerializationComponent(IBuffer buffer)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        private IBufferWriter<byte> Writer => buffer.Writer;

        internal Task HandleAsync(Transmit<Connect> connect, CancellationToken cancellationToken)
        {
            Writer.Write(connect);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<ConnAck> conAck, CancellationToken cancellationToken)
        {
            Writer.Write(conAck);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<Subscribe> subscribe, CancellationToken cancellationToken)
        {
            Writer.Write(subscribe);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<SubAck> subAck, CancellationToken cancellationToken)
        {
            Writer.Write(subAck);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<Publish> publish, CancellationToken cancellationToken)
        {
            Writer.Write(publish);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<PubAck> pubAck, CancellationToken cancellationToken)
        {
            Writer.Write(pubAck);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<PubRec> pubRec, CancellationToken cancellationToken)
        {
            Writer.Write(pubRec);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<PubRel> pubRel, CancellationToken cancellationToken)
        {
            Writer.Write(pubRel);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<PubComp> pubComp, CancellationToken cancellationToken)
        {
            Writer.Write(pubComp);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<PingReq> pingReq, CancellationToken cancellationToken)
        {
            Writer.Write(pingReq);
            return buffer.FlushAsync(cancellationToken);
        }

        internal Task HandleAsync(Transmit<PingResp> pingResp, CancellationToken cancellationToken)
        {
            Writer.Write(pingResp);
            return buffer.FlushAsync(cancellationToken);
        }
    }
}
