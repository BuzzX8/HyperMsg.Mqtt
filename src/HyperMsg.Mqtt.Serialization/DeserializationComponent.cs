using System;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Serialization
{
    public class DeserializationComponent
    {
        private readonly IMessageSender messageSender;

        public DeserializationComponent(IMessageSender messageSender)
        {
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        public Task ProcessBufferFlushAsync(IBufferReader<byte> reader, CancellationToken cancellationToken)
        {
            var buffer = reader.Read();

            if (buffer.Length == 0)
            {
                return Task.CompletedTask;
            }

            var packetRead = buffer.ReadMqttPacket();

            if (packetRead.Item1 == 0)
            {
                return Task.CompletedTask;
            }

            return messageSender.ReceivedAsync<Packet>(packetRead.Item2, cancellationToken);
        }
    }
}
