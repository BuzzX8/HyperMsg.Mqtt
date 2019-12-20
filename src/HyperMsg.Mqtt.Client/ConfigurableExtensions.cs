namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.AddSetting(nameof(MqttConnectionSettings), connectionSettings);
            configurable.RegisterService(typeof(IMqttClient), (p, s) =>
            {               
                var messageSender = (IMessageSender)p.GetService(typeof(IMessageSender));
                var registry = (IMessageHandlerRegistry)p.GetService(typeof(IMessageHandlerRegistry));
                var settings = (MqttConnectionSettings)s[nameof(MqttConnectionSettings)];

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
