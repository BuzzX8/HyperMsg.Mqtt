using System.Collections.Generic;

namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttClient(this IConfigurable configurable, MqttConnectionSettings connectionSettings)
        {
            configurable.AddSetting(nameof(MqttConnectionSettings), connectionSettings);
            configurable.AddService(typeof(IMqttClient), (p, s) =>
            {
                var sender = (ISender<Packet>)p.GetService(typeof(ISender<Packet>));
                var transportHandler = (IHandler<TransportCommands>)p.GetService(typeof(IHandler<TransportCommands>));
                var receiveModeHandler = (IHandler<ReceiveMode>)p.GetService(typeof(IHandler<ReceiveMode>));
                var handlerCollection = (ICollection<IHandler<Packet>>)p.GetService(typeof(ICollection<IHandler<Packet>>));
                var settings = (MqttConnectionSettings)s[nameof(MqttConnectionSettings)];

                var client = new MqttClient(sender, settings, transportHandler, receiveModeHandler);
                handlerCollection.Add(client);
                return client;
            });
        }
    }
}
