namespace HyperMsg.Mqtt.Serialization
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttSerialization(this IConfigurable configurable)
        {
            configurable.AddInitializer(provider =>
            {
                var bufferContext = provider.GetRequiredService<IBufferContext>();
                var messageSender = provider.GetRequiredService<IMessageSender>();
                var handlerRegistry = provider.GetRequiredService<IMessageHandlerRegistry>();
                
                RegisterSerializationHandlers(bufferContext.TransmittingBuffer, handlerRegistry);
                RegisterDeserializationHandler(bufferContext.ReceivingBuffer, messageSender);
            });
        }

        private static void RegisterSerializationHandlers(IBuffer buffer, IMessageHandlerRegistry handlerRegistry)
        {
            var serializer = new SerializationComponent(buffer);
            handlerRegistry.Register<Transmit<Connect>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<ConnAck>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<Subscribe>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<SubAck>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<Publish>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<PubAck>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<PubRec>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<PubRel>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<PubComp>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<PingReq>>(serializer.HandleAsync);
            handlerRegistry.Register<Transmit<PingResp>>(serializer.HandleAsync);
        }

        private static void RegisterDeserializationHandler(IBuffer buffer, IMessageSender messageSender)
        {
            var deserializer = new DeserializationComponent(messageSender);
            buffer.FlushRequested += deserializer.ProcessBufferFlushAsync;
        }
    }
}
