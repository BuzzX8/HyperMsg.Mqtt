using System;

namespace HyperMsg.Mqtt.Packets
{
    public class Publish : IEquatable<Publish>
    {
        public Publish(ushort packetId, string topic, ReadOnlyMemory<byte> message, QosLevel qos)
        {
            Id = packetId;
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Message = message;
            Qos = qos;
        }

        public bool Dup { get; set; }

        public QosLevel Qos { get; }

        public bool Retain { get; set; }

        public string Topic { get; }

        public ushort Id { get; }

        public ReadOnlyMemory<byte> Message { get; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as Publish);

        public bool Equals(Publish packet)
        {
            return packet?.Dup == Dup
                && packet?.Qos == Qos
                && packet?.Retain == Retain
                && packet?.Topic == Topic
                && packet?.Id == Id;
        }
    }
}
