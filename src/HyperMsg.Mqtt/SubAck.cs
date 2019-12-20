using System;
using System.Collections.Generic;

namespace HyperMsg.Mqtt
{
    public class SubAck : IEquatable<SubAck>
    {
        public SubAck(ushort id, IEnumerable<SubscriptionResult> results)
        {
            Id = id;
            Results = results;
        }

        public ushort Id { get; }

		public IEnumerable<SubscriptionResult> Results { get; }

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
