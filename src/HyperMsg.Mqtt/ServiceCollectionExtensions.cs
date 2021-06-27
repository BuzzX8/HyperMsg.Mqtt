using Microsoft.Extensions.DependencyInjection;

namespace HyperMsg.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttServices(this IServiceCollection services) => 
            services.AddHostedService<BufferSerializationService>()
                .AddHostedService<ProtocolService>();
    }
}
