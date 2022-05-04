using Microsoft.Extensions.DependencyInjection;

namespace HyperMsg.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttProtocolService(this IServiceCollection services) =>
            services.AddHostedService<ProtocolService>();

        public static IServiceCollection AddMqttSerialization(this IServiceCollection services) => 
            services.AddHostedService<BufferSerializationService>();
    }
}
