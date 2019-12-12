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

        internal void Handle(Transmit<Packet> transmit)
        {
            buffer.Writer.WriteMqttPacket(transmit);
        }

        internal async Task HandleAsync(Transmit<Connect> transmit, CancellationToken cancellationToken)
        {
            buffer.Writer.WriteMqttPacket(transmit);
            await buffer.FlushAsync(cancellationToken);
        }
    }
}
