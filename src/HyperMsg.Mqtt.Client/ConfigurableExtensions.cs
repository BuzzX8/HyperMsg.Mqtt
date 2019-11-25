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

                var client = new MqttClient(messageSender, connectionSettings);
                registry.Register<Received<Packet>>(client.HandleAsync);

                return client;
            });
        }
    }
}
