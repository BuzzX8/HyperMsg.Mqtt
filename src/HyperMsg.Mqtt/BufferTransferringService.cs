using System;
using System.Collections.Generic;
using static HyperMsg.Mqtt.MqttSerializer;
using static HyperMsg.Mqtt.MqttDeserializer;
using HyperMsg.Mqtt.Packets;
using System.Threading.Tasks;
using System.Buffers;
using System.Threading;

namespace HyperMsg.Mqtt
{
    internal class BufferTransferringService : MessagingService
    {
        public BufferTransferringService(IMessagingContext messagingContext) : base(messagingContext)
        {
        }

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterSerializationHandler<Connect>(Serialize);
            yield return this.RegisterSerializationHandler<ConnAck>(Serialize);
            yield return this.RegisterSerializationHandler<Subscribe>(Serialize);
            yield return this.RegisterSerializationHandler<SubAck>(Serialize);
            yield return this.RegisterSerializationHandler<Unsubscribe>(Serialize);
            yield return this.RegisterSerializationHandler<UnsubAck>(Serialize);
            yield return this.RegisterSerializationHandler<Publish>(Serialize);
            yield return this.RegisterSerializationHandler<PubAck>(Serialize);
            yield return this.RegisterSerializationHandler<PubRec>(Serialize);
            yield return this.RegisterSerializationHandler<PubRel>(Serialize);
            yield return this.RegisterSerializationHandler<PubComp>(Serialize);
            yield return this.RegisterSerializationHandler<PingReq>(Serialize);
            yield return this.RegisterSerializationHandler<PingResp>(Serialize);
            yield return this.RegisterSerializationHandler<Disconnect>(Serialize);

            yield return this.RegisterBufferFlushReader(BufferType.Receiving, ReadReceivingBufferAsync);
        }

        private Task<int> ReadReceivingBufferAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken) => ReadBufferAsync(this, data, cancellationToken);
    }
}
