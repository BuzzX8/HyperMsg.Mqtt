using System;

namespace HyperMsg.Mqtt
{
    public class UnsubAck : IEquatable<UnsubAck>
    {
	    public UnsubAck(ushort id)
	    {
		    Id = id;
	    }

		public ushort Id { get; }
		
	    public override int GetHashCode() => Id;

	    public bool Equals(UnsubAck other) => Id == other?.Id;

	    public override bool Equals(object obj) => Equals(obj as UnsubAck);
    }
}
