using System;

namespace HyperMsg.Mqtt
{
    public class SubAck : Packet, IEquatable<SubAck>
    {
        public SubAck(ushort id) => Id = id;

        public SubAck(ushort id, params SubscriptionResult[] results) : this(id) => Results = results;

        public ushort Id { get; }

		public SubscriptionResult[] Results { get; set; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as SubAck);

	    public bool Equals(SubAck packet)
	    {
		    if (packet == null)
		    {
			    return false;
		    }

		    return Id == packet.Id;
	    }
    }
}
