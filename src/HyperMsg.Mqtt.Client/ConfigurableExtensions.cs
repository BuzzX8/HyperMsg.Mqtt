namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.AddSetting(nameof(MqttConnectionSettings), connectionSettings);
            configurable.RegisterService(typeof(IMqttClient), (p, s) =>
            {
                var sender = (ISender<Packet>)p.GetService(typeof(ISender<Packet>));
                var handler = (IHandler)p.GetService(typeof(IHandler));
                var repository = (IHandlerRepository)p.GetService(typeof(IHandlerRepository));
                var settings = (MqttConnectionSettings)s[nameof(MqttConnectionSettings)];

                var client = new MqttClient(sender, settings, handler);
                repository.AddHandler(client);

                return client;
            });
        }
    }
}
