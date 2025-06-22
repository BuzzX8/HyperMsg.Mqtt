using HyperMsg.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace HyperMsg.Mqtt.Coding;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMqttCoding(this IServiceCollection services)
    {        
        return services.AddMqttDecoding()
            .AddMqttEncoding();
    }

    public static IServiceCollection AddMqttEncoding(this IServiceCollection services)
    {
        var component = new EncodingComponent(null);
        return services.AddMessagingComponent(component);
    }

    public static IServiceCollection AddMqttDecoding(this IServiceCollection services)
    {
        var component = new DecodingComponent();
        return services.AddMessagingComponent(component);
    }
}
