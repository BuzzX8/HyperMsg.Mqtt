namespace HyperMsg.Mqtt.Serialization
{
    public static class ConfigurableExtensions
    {
        public static void AddMqttSerialization(this IConfigurable configurable)
        {
            configurable.AddInitializer(provider =>
            {
                var bufferContext = provider.GetRequiredService<IBufferContext>();
                var messageSender = provider.GetRequiredService<IMessageSender>();
                var observable = provider.GetRequiredService<IMessageObservable>();
                
                AddSerializationComponent(bufferContext.TransmittingBuffer, observable);
                AddDeserializationComponent(bufferContext.ReceivingBuffer, messageSender);
            });
        }

        private static void AddSerializationComponent(IBuffer buffer, IMessageObservable handlerRegistry)
        {
            var serializer = new SerializationComponent(buffer);
            handlerRegistry.Subscribe<Transmit<Connect>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<ConnAck>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<Subscribe>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<SubAck>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<Publish>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<PubAck>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<PubRec>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<PubRel>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<PubComp>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<PingReq>>(serializer.HandleAsync);
            handlerRegistry.Subscribe<Transmit<PingResp>>(serializer.HandleAsync);
        }

        private static void AddDeserializationComponent(IBuffer buffer, IMessageSender messageSender)
        {
            var deserializer = new DeserializationComponent(messageSender);
            buffer.FlushRequested += deserializer.ProcessBufferFlushAsync;
        }
    }
}
