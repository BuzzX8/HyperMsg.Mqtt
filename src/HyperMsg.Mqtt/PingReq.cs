using System;

namespace HyperMsg.Mqtt
{
	public class PingReq : Packet, IEquatable<PingReq>
	{
		public override int GetHashCode() => PacketCodes.PingReq;

		public override bool Equals(object obj) => Equals(obj as PingReq);

		public bool Equals(PingReq packet) => packet != null;

		public override string ToString() => "PingReq";
	}
}
