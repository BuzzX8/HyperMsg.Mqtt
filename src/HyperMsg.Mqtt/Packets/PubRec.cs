using System;

namespace HyperMsg.Mqtt.Packets
{
    public class PubRec : IEquatable<PubRec>
    {
        public PubRec(ushort id)
        {
            Id = id;
        }

        public ushort Id { get; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as PubRec);

        public bool Equals(PubRec packet) => packet?.Id == Id;
    }
}
