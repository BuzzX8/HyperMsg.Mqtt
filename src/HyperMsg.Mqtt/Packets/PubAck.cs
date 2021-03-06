﻿using System;

namespace HyperMsg.Mqtt.Packets
{
    public class PubAck : IEquatable<PubAck>
    {
        public PubAck(ushort id)
        {
            Id = id;
        }

        public ushort Id { get; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as PubAck);

        public bool Equals(PubAck packet) => packet.Id == Id;
    }
}
