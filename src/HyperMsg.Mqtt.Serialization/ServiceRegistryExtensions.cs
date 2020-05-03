namespace HyperMsg.Mqtt.Serialization
{
    public static class ServiceRegistryExtensions
    {
        public static void AddMqttSerialization(this IServiceRegistry serviceRegistry)
        {
            serviceRegistry.AddService(provider =>
            {
                var context = provider.GetRequiredService<IMessagingContext>();
                return new DeserializationService(context.Sender, context.Observable);
            });
            AddSerializers(serviceRegistry);
        }

        private static void AddSerializers(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.AddSerializer<Connect>(SerializationsExtensions.Write);
            serviceRegistry.AddSerializer<Subscribe>(SerializationsExtensions.Write);
        }
    }
}
