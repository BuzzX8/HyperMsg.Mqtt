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

                var serializer = new SerializationComponent(transmittingBuffer);
                handlerRegistry.Register<Transmit<Packet>>(serializer.Handle);

                var deserializer = new DeserializationComponent(messageSender);
                receivingBuffer.FlushRequested += deserializer.ProcessBufferFlushAsync;
            });
        }
    }
}
