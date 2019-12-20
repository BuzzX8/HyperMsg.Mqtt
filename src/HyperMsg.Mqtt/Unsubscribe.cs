using System;
using System.Collections.Generic;

namespace HyperMsg.Mqtt
{
    public class Unsubscribe : IEquatable<Unsubscribe>
    {
        public Unsubscribe(ushort id, IEnumerable<string> topics)
        {
            Id = id;
            Topics = topics ?? throw new ArgumentNullException(nameof(topics));
        }

        public ushort Id { get; }

		public IEnumerable<string> Topics { get; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as Unsubscribe);

	    public bool Equals(Unsubscribe packet)
	    {
		    if (packet == null)
		    {
			    return false;
		    }

		    return Id == packet.Id;
	    }
    }
}
