using System;

namespace HyperMsg.Mqtt.Packets
{
    public class PubRel : IEquatable<PubRel>
    {
        public PubRel(ushort id)
        {
            Id = id;
        }

        public ushort Id { get; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as PubRel);

        public bool Equals(PubRel packet) => packet?.Id == Id;
    }
}
