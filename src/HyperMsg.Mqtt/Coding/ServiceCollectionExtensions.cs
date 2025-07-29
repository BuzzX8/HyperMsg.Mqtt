using HyperMsg.Coding;
using Microsoft.Extensions.DependencyInjection;

namespace HyperMsg.Mqtt.Coding;

/// <summary>
/// Provides extension methods for registering MQTT coding services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the MQTT encoding and decoding context with the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the coding context to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddMqttCoding(this IServiceCollection services) 
        => services.AddCodingContext(Encoding.Encode, Decoding.Decode);
}
