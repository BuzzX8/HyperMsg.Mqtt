using HyperMsg.Extensions;
using Microsoft.Extensions.DependencyInjection;
using static HyperMsg.Mqtt.Serialization.MqttSerializer;

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
    }
}
