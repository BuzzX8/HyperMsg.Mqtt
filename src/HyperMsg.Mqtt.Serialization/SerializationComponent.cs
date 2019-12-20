using System;
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

        internal async Task HandleAsync(Transmit<Connect> connect, CancellationToken cancellationToken)
        {
            buffer.Writer.WriteMqttPacket(connect);
            await buffer.FlushAsync(cancellationToken);
        }

        internal async Task HandleAsync(Transmit<ConnAck> conAck, CancellationToken cancellationToken)
        {

        }

        internal async Task HandleAsync(Transmit<Subscribe> subscribe, CancellationToken cancellationToken)
        {

        }

        internal async Task HandleAsync(Transmit<SubAck> subAck, CancellationToken cancellationToken)
        {

        }
    }
}
