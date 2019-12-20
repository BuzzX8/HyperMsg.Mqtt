using System;

namespace HyperMsg.Mqtt
{
	public class Disconnect : IEquatable<Disconnect>
	{
        public static readonly Disconnect Instance = new Disconnect();

		public override int GetHashCode() => PacketCodes.Disconnect;

		public override bool Equals(object obj) => Equals(obj as Disconnect);

		public bool Equals(Disconnect packet) => packet != null;

		public override string ToString() => "Disconnect";
	}
}
