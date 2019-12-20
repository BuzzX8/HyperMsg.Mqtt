namespace HyperMsg.Mqtt.Serialization
{
    public static class ConfigurableExtensions
    {
        public static void UseMqttSerialization(this IConfigurable configurable)
        {
            configurable.RegisterConfigurator((p, s) =>
            {
                var transmittingBuffer = (ITransmittingBuffer)p.GetService(typeof(ITransmittingBuffer));
                var receivingBuffer = (IReceivingBuffer)p.GetService(typeof(IReceivingBuffer));
                var messageSender = (IMessageSender)p.GetService(typeof(IMessageSender));
                var handlerRegistry = (IMessageHandlerRegistry)p.GetService(typeof(IMessageHandlerRegistry));
                
                RegisterSerializationHandlers(transmittingBuffer, handlerRegistry);

                var deserializer = new DeserializationComponent(messageSender);
                receivingBuffer.FlushRequested += deserializer.ProcessBufferFlushAsync;
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
    }
}
