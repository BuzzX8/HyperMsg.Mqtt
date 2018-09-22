using System;

namespace HyperMsg.Mqtt
{
    public class Publish : Packet, IEquatable<Publish>
    {
        public Publish(ushort id) => Id = id;

        public Publish(ushort packetId, params byte[] message) : this(packetId) => Message = message;

        public bool Dup { get; set; }

		public QosLevel Qos { get; set; }

		public bool Retain { get; set; }

		public string Topic { get; set; }

		public ushort Id { get; }

		public byte[] Message { get; set; }

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
