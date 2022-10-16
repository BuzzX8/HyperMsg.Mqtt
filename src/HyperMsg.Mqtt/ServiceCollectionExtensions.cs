using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.DependencyInjection;
using static HyperMsg.Mqtt.MqttSerializer;
using static HyperMsg.Mqtt.MqttDeserializer;

namespace HyperMsg.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttProtocolService(this IServiceCollection services) =>
            services.AddHostedService<ProtocolService>();

        public static IServiceCollection AddMqttSerialization(this IServiceCollection services)
        {            
            var compositeSerializer = new CompositeSerializer();
            RegisterSerializers(compositeSerializer);

            return services.AddSerializer(compositeSerializer)
                .AddDeserializer(ReadBuffer);
        }

        private static void RegisterSerializers(CompositeSerializer compositeSerializer)
        {
            compositeSerializer.Register<Connect>(Serialize);
            compositeSerializer.Register<ConnAck>(Serialize);
            compositeSerializer.Register<Subscribe>(Serialize);
            compositeSerializer.Register<SubAck>(Serialize);
            compositeSerializer.Register<Unsubscribe>(Serialize);
            compositeSerializer.Register<UnsubAck>(Serialize);
            compositeSerializer.Register<Publish>(Serialize);
            compositeSerializer.Register<PubAck>(Serialize);
            compositeSerializer.Register<PubRec>(Serialize);
            compositeSerializer.Register<PubRel>(Serialize);
            compositeSerializer.Register<PubComp>(Serialize);
            compositeSerializer.Register<PingReq>(Serialize);
            compositeSerializer.Register<PingResp>(Serialize);
            compositeSerializer.Register<Disconnect>(Serialize);
        }

        //private static void DeregisterSerializers(CompositeSerializer compositeSerializer)
        //{
        //    compositeSerializer.Deregister<Connect>();
        //    compositeSerializer.Deregister<ConnAck>();
        //    compositeSerializer.Deregister<Subscribe>();
        //    compositeSerializer.Deregister<SubAck>();
        //    compositeSerializer.Deregister<Unsubscribe>();
        //    compositeSerializer.Deregister<UnsubAck>();
        //    compositeSerializer.Deregister<Publish>();
        //    compositeSerializer.Deregister<PubAck>();
        //    compositeSerializer.Deregister<PubRec>();
        //    compositeSerializer.Deregister<PubRel>();
        //    compositeSerializer.Deregister<PubComp>();
        //    compositeSerializer.Deregister<PingReq>();
        //    compositeSerializer.Deregister<PingResp>();
        //    compositeSerializer.Deregister<Disconnect>();
        //}
    }
}
