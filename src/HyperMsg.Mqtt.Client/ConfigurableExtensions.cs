namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.AddSetting(nameof(MqttConnectionSettings), connectionSettings);
            configurable.Configure(c =>
            {
                var sender = (ISender<Packet>)c.GetService(typeof(ISender<Packet>));
                var transportHandler = (IHandler<TransportCommands>)c.GetService(typeof(IHandler<TransportCommands>));
                var receiveModeHandler = (IHandler<ReceiveMode>)c.GetService(typeof(IHandler<ReceiveMode>));
                var settings = (MqttConnectionSettings)c.GetSetting(nameof(MqttConnectionSettings));

                var client = new MqttClient(sender, settings)
                {
                    SetReceiveModeAsync = receiveModeHandler.HandleAsync,
                    SubmitTransportCommandAsync = transportHandler.HandleAsync
                };

                c.RegisterService(typeof(IMqttClient), client);
            });
        }
    }
}
