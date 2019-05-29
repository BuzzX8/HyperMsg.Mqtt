namespace HyperMsg.Mqtt.Serialization
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttSerializer(this IConfigurable configurable) => configurable.RegisterService(typeof(ISerializer<Packet>), (p, s) => new MqttSerializer());
    }
}
