namespace HyperMsg.Mqtt.Serialization
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttSerializer(this IConfigurable configurable)
        {
            configurable.Configure(context =>
            {
                context.RegisterService(typeof(ISerializer<Packet>), new MqttSerializer());
            });
        }
    }
}
