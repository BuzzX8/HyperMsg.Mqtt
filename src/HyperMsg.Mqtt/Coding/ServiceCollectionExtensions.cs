using HyperMsg.Buffers;
using HyperMsg.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace HyperMsg.Mqtt.Coding;

/// <summary>
/// Extension methods for registering MQTT coding components with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers both MQTT encoding and decoding components in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the components to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMqttCoding(this IServiceCollection services)
    {        
        return services
            .AddMqttDecoding()
            .AddMqttEncoding();
    }

    /// <summary>
    /// Registers the MQTT encoding component in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the encoding component to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMqttEncoding(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Registers the MQTT decoding component in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the decoding component to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMqttDecoding(this IServiceCollection services)
    {
        return services;
    }
}
