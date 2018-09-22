using System;

namespace HyperMsg.Mqtt
{
    public class ConnAck : Packet, IEquatable<ConnAck>
    {
		public ConnAck(ConnectionResult resultCode, bool sessionPresent = false)
		{
			ResultCode = resultCode;
			SessionPresent = sessionPresent;
		}

		public ConnectionResult ResultCode { get; }

		public bool SessionPresent { get; }

		public override int GetHashCode() => GetType().GetHashCode();

		public override bool Equals(object obj) => Equals(obj as ConnAck);

		public bool Equals(ConnAck packet)
		{
			return packet?.SessionPresent == SessionPresent
				&& packet?.ResultCode == ResultCode;
		}

		public override string ToString() => $"ConnAck(SP={SessionPresent},Code={ResultCode})";
	}
}
