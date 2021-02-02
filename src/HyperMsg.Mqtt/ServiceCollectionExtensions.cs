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
            return services.AddSerializer<Connect>(Serialize)
                .AddSerializer<ConnAck>(Serialize)
                .AddSerializer<Subscribe>(Serialize)
                .AddSerializer<SubAck>(Serialize)
                .AddSerializer<Unsubscribe>(Serialize)
                .AddSerializer<UnsubAck>(Serialize)
                .AddSerializer<Publish>(Serialize)
                .AddSerializer<PubAck>(Serialize)
                .AddSerializer<PubRec>(Serialize)
                .AddSerializer<PubRel>(Serialize)
                .AddSerializer<PubComp>(Serialize)
                .AddSerializer<PingReq>(Serialize)
                .AddSerializer<PingResp>(Serialize)
                .AddSerializer<Disconnect>(Serialize);
        }

        public static IServiceCollection AddMqttDeserializers(this IServiceCollection services) => services.AddDeserializer(Deserialize);

        public static IServiceCollection AddMqttServices(this IServiceCollection services)
        {
            return services.AddMqttSerializers()
                .AddMqttDeserializers();         
        }
    }
}
