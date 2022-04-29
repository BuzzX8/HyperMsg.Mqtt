using Microsoft.Extensions.DependencyInjection;

namespace HyperMsg.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttSerialization(this IServiceCollection services) => 
            services.AddHostedService<BufferSerializationService>();
    }
}
