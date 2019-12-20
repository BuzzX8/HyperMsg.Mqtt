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

            if (packetRead.BytesConsumed == 0)
            {
                return Task.CompletedTask;
            }

            switch(packetRead.Packet)
            {
                case Connect connect:
                    return messageSender.ReceivedAsync(connect, cancellationToken);

                case ConnAck connAck:
                    return messageSender.ReceivedAsync(connAck, cancellationToken);

                case Subscribe subscribe:
                    return messageSender.ReceivedAsync(subscribe, cancellationToken);

                case SubAck subAck:
                    return messageSender.ReceivedAsync(subAck, cancellationToken);

                case Publish publish:
                    return messageSender.ReceivedAsync(publish, cancellationToken);

                case PubAck pubAck:
                    return messageSender.ReceivedAsync(pubAck, cancellationToken);

                case PubRec pubRec:
                    return messageSender.ReceivedAsync(pubRec, cancellationToken);

                case PubRel pubRel:
                    return messageSender.ReceivedAsync(pubRel, cancellationToken);

                case PubComp pubComp:
                    return messageSender.ReceivedAsync(pubComp, cancellationToken);

                case PingReq pingReq:
                    return messageSender.ReceivedAsync(pingReq, cancellationToken);

                case PingResp pingResp:
                    return messageSender.ReceivedAsync(pingResp, cancellationToken);

                case Disconnect disconnect:
                    return messageSender.ReceivedAsync(disconnect, cancellationToken);
            }

            throw new NotSupportedException();
        }
    }
}
