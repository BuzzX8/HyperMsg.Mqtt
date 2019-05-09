namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.Configure(c =>
            {
                var descriptor = ServiceDescriptor.Describe(typeof(IMqttClient), provider =>
                {
                    var sender = (ISender<Packet>)provider.GetService(typeof(ISender<Packet>));
                    var transportHandler = (IHandler<TransportCommands>)provider.GetService(typeof(IHandler<TransportCommands>));
                    var receiveModeHandler = (IHandler<ReceiveMode>)provider.GetService(typeof(IHandler<ReceiveMode>));
                    var settings = (MqttConnectionSettings)c.Settings[nameof(MqttConnectionSettings)];

                    return new MqttClient(sender, settings)
                    {
                        SetReceiveModeAsync = receiveModeHandler.HandleAsync,
                        SubmitTransportCommandAsync = transportHandler.HandleAsync
                    };
                });
                c.Services.Add(descriptor);
            });
        }
    }
}
