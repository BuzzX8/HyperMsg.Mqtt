using System;

namespace HyperMsg.Mqtt.Packets
{
    public class PingResp : IEquatable<PingResp>
    {
        public static readonly PingResp Instance = new PingResp();

        public override int GetHashCode() => PacketCodes.PingResp;

        public override bool Equals(object obj) => Equals(obj as PingResp);

        public bool Equals(PingResp packet) => packet != null;

        public override string ToString() => "PingResp";
    }
}
