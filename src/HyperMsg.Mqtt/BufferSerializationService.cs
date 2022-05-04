using static HyperMsg.Mqtt.MqttSerializer;
using static HyperMsg.Mqtt.MqttDeserializer;
using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;

namespace HyperMsg.Mqtt
{
    internal class BufferSerializationService : IHostedService
    {
        private readonly IBroker receiver;
        private readonly SerializationFilter serializationFilter;

        public BufferSerializationService(IContext context, SerializationFilter serializationFilter) => 
            (receiver, this.serializationFilter) = (context.Receiver, serializationFilter);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            RegisterSerializers();
            RegisterDeserializer();

            return Task.CompletedTask;
        }

        private void RegisterDeserializer() => receiver.Registry.Register<BufferUpdatedEvent>(ReceiveBufferUpdateEventHandler);

        private void RegisterSerializers()
        {
            serializationFilter.Register<Connect>(Serialize);
            serializationFilter.Register<ConnAck>(Serialize);
            serializationFilter.Register<Subscribe>(Serialize);
            serializationFilter.Register<SubAck>(Serialize);
            serializationFilter.Register<Unsubscribe>(Serialize);
            serializationFilter.Register<UnsubAck>(Serialize);
            serializationFilter.Register<Publish>(Serialize);
            serializationFilter.Register<PubAck>(Serialize);
            serializationFilter.Register<PubRec>(Serialize);
            serializationFilter.Register<PubRel>(Serialize);
            serializationFilter.Register<PubComp>(Serialize);
            serializationFilter.Register<PingReq>(Serialize);
            serializationFilter.Register<PingResp>(Serialize);
            serializationFilter.Register<Disconnect>(Serialize);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            DeregisterSerializers();
            DeregisterDeserializer();

            return Task.CompletedTask;
        }

        private void DeregisterDeserializer() => receiver.Registry.Deregister<BufferUpdatedEvent>(ReceiveBufferUpdateEventHandler);

        private void DeregisterSerializers()
        {
            serializationFilter.Deregister<Connect>();
            serializationFilter.Deregister<ConnAck>();
            serializationFilter.Deregister<Subscribe>();
            serializationFilter.Deregister<SubAck>();
            serializationFilter.Deregister<Unsubscribe>();
            serializationFilter.Deregister<UnsubAck>();
            serializationFilter.Deregister<Publish>();
            serializationFilter.Deregister<PubAck>();
            serializationFilter.Deregister<PubRec>();
            serializationFilter.Deregister<PubRel>();
            serializationFilter.Deregister<PubComp>();
            serializationFilter.Deregister<PingReq>();
            serializationFilter.Deregister<PingResp>();
            serializationFilter.Deregister<Disconnect>();
        }

        private void ReceiveBufferUpdateEventHandler(BufferUpdatedEvent @event) => ReadBufferAsync(receiver, @event.Buffer.Reader);
    }
}
