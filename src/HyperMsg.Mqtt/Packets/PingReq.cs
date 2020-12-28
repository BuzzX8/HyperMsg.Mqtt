using System;

namespace HyperMsg.Mqtt.Packets
{
    public class PingReq : IEquatable<PingReq>
	{
        public static readonly PingReq Instance = new PingReq();

		public override int GetHashCode() => PacketCodes.PingReq;

		public override bool Equals(object obj) => Equals(obj as PingReq);

		public bool Equals(PingReq packet) => packet != null;

		public override string ToString() => "PingReq";
	}
}
