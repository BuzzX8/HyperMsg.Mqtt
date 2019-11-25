using System;

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
    }
}
