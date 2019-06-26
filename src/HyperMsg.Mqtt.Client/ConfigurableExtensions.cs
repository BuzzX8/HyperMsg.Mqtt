namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.AddSetting(nameof(MqttConnectionSettings), connectionSettings);
            configurable.RegisterService(typeof(IMqttClient), (p, s) =>
            {
                var transportCommandSender = (ITransportCommandSender)p.GetService(typeof(ITransportCommandSender));
                var messageSender = (IMessageSender<Packet>)p.GetService(typeof(IMessageSender<Packet>));
                var repository = (IHandlerRegistry)p.GetService(typeof(IHandlerRegistry));
                var settings = (MqttConnectionSettings)s[nameof(MqttConnectionSettings)];

                var connection = new ConnectionController(transportCommandSender, messageSender, settings);
                var client = new MqttClient(connection, messageSender);
                repository.Register(client);

                return client;
            });
        }
    }
}
