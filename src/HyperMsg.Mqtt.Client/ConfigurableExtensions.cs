namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.AddSetting(nameof(MqttConnectionSettings), connectionSettings);
            configurable.RegisterService(typeof(IMqttClient), (p, s) =>
            {
                var transport = (ITransport)p.GetService(typeof(ITransport));
                var messageSender = (IMessageSender<Packet>)p.GetService(typeof(IMessageSender<Packet>));
                var repository = (IMessageHandlerRegistry<Packet>)p.GetService(typeof(IMessageHandlerRegistry<Packet>));
                var settings = (MqttConnectionSettings)s[nameof(MqttConnectionSettings)];

                var connection = new MqttConnection(transport.ProcessCommandAsync, messageSender, settings);
                var client = new MqttClient(connection, messageSender);
                repository.Register(client.HandleAsync);

                return client;
            });
        }
    }
}
