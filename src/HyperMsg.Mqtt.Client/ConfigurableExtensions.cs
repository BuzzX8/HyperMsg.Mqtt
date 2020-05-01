namespace HyperMsg.Mqtt.Client
{
    public static class ConfigurableExtensions
    {
        public static void AddMqttClient(this IServiceRegistry configurable, MqttConnectionSettings connectionSettings)
        {            
            configurable.AddService(provider =>
            {               
                var messageSender = provider.GetRequiredService<IMessageSender>();
                var observable = provider.GetService<IMessageObservable>();

                return RegisterClient(messageSender, observable, connectionSettings);
            });
        }

        private static IMqttClient RegisterClient(IMessageSender messageSender, IMessageObservable observable, MqttConnectionSettings connectionSettings)
        {
            var client = new MqttClient(messageSender, connectionSettings);

            observable.Subscribe<Received<ConnAck>>(client.Handle);
            observable.Subscribe<Received<Publish>>(client.HandleAsync);
            observable.Subscribe<Received<PubAck>>(client.Handle);
            observable.Subscribe<Received<PubRec>>(client.HandleAsync);
            observable.Subscribe<Received<PubRel>>(client.HandleAsync);
            observable.Subscribe<Received<PubComp>>(client.Handle);
            observable.Subscribe<Received<SubAck>>(client.Handle);
            observable.Subscribe<Received<PingResp>>(client.Handle);
            observable.Subscribe<Received<UnsubAck>>(client.Handle);

            return client;
        }
    }
}
