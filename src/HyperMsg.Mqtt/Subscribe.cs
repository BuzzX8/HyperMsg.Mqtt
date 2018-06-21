using System;

namespace HyperMsg.Mqtt
{
    public class Subscribe : Packet, IEquatable<Subscribe>
	{
        public Subscribe(ushort id) => Id = id;

        public Subscribe(ushort packetId, params (string, QosLevel)[] subscriptions) : this(packetId) => Subscriptions = subscriptions;

        public ushort Id { get; }

		public (string, QosLevel)[] Subscriptions { get; set; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as Subscribe);

		public bool Equals(Subscribe packet) => packet?.Id == Id;
	}
}
