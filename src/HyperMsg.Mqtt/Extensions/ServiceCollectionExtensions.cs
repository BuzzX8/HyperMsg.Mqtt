using HyperMsg.Extensions;
using Microsoft.Extensions.DependencyInjection;
using static HyperMsg.Mqtt.Serialization.MqttSerializer;
using static HyperMsg.Mqtt.Serialization.MqttDeserializer;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Mqtt.Serialization;

namespace HyperMsg.Mqtt.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttSerializers(this IServiceCollection services)
        {
            return services.AddTransmittingBufferSerializer<Connect>(Serialize)
                .AddTransmittingBufferSerializer<ConnAck>(Serialize)
                .AddTransmittingBufferSerializer<Subscribe>(Serialize)
                .AddTransmittingBufferSerializer<SubAck>(Serialize)
                .AddTransmittingBufferSerializer<Unsubscribe>(Serialize)
                .AddTransmittingBufferSerializer<UnsubAck>(Serialize)
                .AddTransmittingBufferSerializer<Publish>(Serialize)
                .AddTransmittingBufferSerializer<PubAck>(Serialize)
                .AddTransmittingBufferSerializer<PubRec>(Serialize)
                .AddTransmittingBufferSerializer<PubRel>(Serialize)
                .AddTransmittingBufferSerializer<PubComp>(Serialize)
                .AddTransmittingBufferSerializer<PingReq>(Serialize)
                .AddTransmittingBufferSerializer<PingResp>(Serialize)
                .AddTransmittingBufferSerializer<Disconnect>(Serialize);
        }

        public static IServiceCollection AddMqttDeserializers(this IServiceCollection services)
        {
            return services.AddSerializationService()
                .AddConfigurator(provider =>
                {
                    var service = provider.GetRequiredService<BufferTransferingService>();
                    var messageSender = provider.GetRequiredService<IMessageSender>();
                    service.AddReceivingBufferReader((buffer, token) => ReadBufferAsync(messageSender, buffer, token));
                });
        }

        public static IServiceCollection AddMqttServices(this IServiceCollection services)
        {
            return services.AddMqttSerializers()
                .AddMqttDeserializers();
        }
    }
}
