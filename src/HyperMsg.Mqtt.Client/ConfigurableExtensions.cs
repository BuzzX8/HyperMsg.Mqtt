namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.AddSetting(nameof(MqttConnectionSettings), connectionSettings);
            configurable.Configure(context =>
            {
                var sender = (ISender<Packet>)context.GetService(typeof(ISender<Packet>));
                var transportHandler = (IHandler<TransportCommands>)context.GetService(typeof(IHandler<TransportCommands>));
                var receiveModeHandler = (IHandler<ReceiveMode>)context.GetService(typeof(IHandler<ReceiveMode>));
                var settings = (MqttConnectionSettings)context.GetSetting(nameof(MqttConnectionSettings));

                var client = new MqttClient(sender, settings)
                {
                    SetReceiveModeAsync = receiveModeHandler.HandleAsync,
                    SubmitTransportCommandAsync = transportHandler.HandleAsync
                };

                context.RegisterService(typeof(IMqttClient), client);
            });
        }
    }
}
