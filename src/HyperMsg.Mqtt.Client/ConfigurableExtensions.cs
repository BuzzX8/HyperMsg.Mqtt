namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void AddMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {            
            configurable.AddService(provider =>
            {               
                var messageSender = provider.GetRequiredService<IMessageSender>();
                var registry = provider.GetService<IMessageHandlerRegistry>();

                return RegisterClient(messageSender, registry, connectionSettings);
            });
        }

        private static IMqttClient RegisterClient(IMessageSender messageSender, IMessageHandlerRegistry registry, MqttConnectionSettings connectionSettings)
        {
            var client = new MqttClient(messageSender, connectionSettings);

            registry.Register<Received<ConnAck>>(client.Handle);
            registry.Register<Received<Publish>>(client.HandleAsync);
            registry.Register<Received<PubAck>>(client.Handle);
            registry.Register<Received<PubRec>>(client.HandleAsync);
            registry.Register<Received<PubRel>>(client.HandleAsync);
            registry.Register<Received<PubComp>>(client.Handle);
            registry.Register<Received<SubAck>>(client.Handle);
            registry.Register<Received<PingResp>>(client.Handle);
            registry.Register<Received<UnsubAck>>(client.Handle);

            return client;
        }
    }
}
