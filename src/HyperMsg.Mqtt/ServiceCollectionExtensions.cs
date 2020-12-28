using HyperMsg.Extensions;
using Microsoft.Extensions.DependencyInjection;
using static HyperMsg.Mqtt.Serialization.MqttSerializer;
using static HyperMsg.Mqtt.Serialization.MqttDeserializer;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttSerializers(this IServiceCollection services)
        {
            return services.AddSerializationComponent<Connect>(Serialize)
                .AddSerializationComponent<ConnAck>(Serialize)
                .AddSerializationComponent<Subscribe>(Serialize)
                .AddSerializationComponent<SubAck>(Serialize)
                .AddSerializationComponent<Unsubscribe>(Serialize)
                .AddSerializationComponent<UnsubAck>(Serialize)
                .AddSerializationComponent<Publish>(Serialize)
                .AddSerializationComponent<PubAck>(Serialize)
                .AddSerializationComponent<PubRec>(Serialize)
                .AddSerializationComponent<PubRel>(Serialize)
                .AddSerializationComponent<PubComp>(Serialize)
                .AddSerializationComponent<PingReq>(Serialize)
                .AddSerializationComponent<PingResp>(Serialize)
                .AddSerializationComponent<Disconnect>(Serialize);
        }

        public static IServiceCollection AddMqttDeserializers(this IServiceCollection services)
        {
            return services.AddDeserializationComponent(Deserialize);
        }

        public static IServiceCollection AddMqttSettings(this IServiceCollection services, MqttConnectionSettings connectionSettings) => services.AddSingleton(connectionSettings);

        public static IServiceCollection AddMqttComponents(this IServiceCollection services)
        {
            return services.AddObserver<ConnectionComponent, ConnAck>(component => component.Handle);
        }

        public static IServiceCollection AddMqttServices(this IServiceCollection services, MqttConnectionSettings connectionSettings)
        {
            return services.AddMqttSerializers()
                .AddMqttDeserializers()
                .AddMqttComponents()
                .AddMqttSettings(connectionSettings);
        }
    }
}
