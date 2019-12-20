using System;
using System.Collections.Generic;

namespace HyperMsg.Mqtt
{
    public class Subscribe : IEquatable<Subscribe>
	{
        public Subscribe(ushort packetId, IEnumerable<(string, QosLevel)> subscriptions)
        {
            Id = packetId;
            Subscriptions = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
        }

        public ushort Id { get; }

		public IEnumerable<(string, QosLevel)> Subscriptions { get; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as Subscribe);

		public bool Equals(Subscribe packet) => packet?.Id == Id;
	}
}
