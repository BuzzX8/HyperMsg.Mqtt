using System;

namespace HyperMsg.Mqtt
{
    public class PubComp : IEquatable<PubComp>
    {
	    public PubComp(ushort packetId)
	    {
		    Id = packetId;
	    }

		public ushort Id { get; }

	    public override int GetHashCode() => Id;

	    public override bool Equals(object obj) => Equals(obj as PubComp);

	    public bool Equals(PubComp packet)
	    {
		    return packet?.Id == Id;
	    }
    }
}
