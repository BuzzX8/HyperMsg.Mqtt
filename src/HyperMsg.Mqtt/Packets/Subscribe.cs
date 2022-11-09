﻿using System;
using System.Collections.Generic;

namespace HyperMsg.Mqtt.Packets
{
    public class Subscribe : IEquatable<Subscribe>
    {
        public Subscribe(ushort packetId, IEnumerable<SubscriptionRequest> subscriptions)
        {
            Id = packetId;
            Subscriptions = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
        }

        public ushort Id { get; }

        public IEnumerable<SubscriptionRequest> Subscriptions { get; }

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals(obj as Subscribe);

        public bool Equals(Subscribe packet) => packet?.Id == Id;
    }
}
