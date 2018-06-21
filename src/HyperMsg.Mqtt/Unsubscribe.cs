using System;

namespace HyperMsg.Mqtt
{
    public class Unsubscribe : Packet, IEquatable<Unsubscribe>
    {
        public Unsubscribe(ushort id) => Id = id;

        public Unsubscribe(ushort id, params string[] topics) : this(id) => Topics = topics;

        public ushort Id { get; }

		public string[] Topics { get; set; }

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
