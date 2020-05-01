namespace HyperMsg.Mqtt.Serialization
{
    public static class ConfigurableExtensions
    {
        //public static void AddMqttSerialization(this IServiceRegistry configurable)
        //{
        //    configurable.AddInitializer(provider =>
        //    {
        //        var bufferContext = provider.GetRequiredService<IBufferContext>();
        //        var messageSender = provider.GetRequiredService<IMessageSender>();
        //        var observable = provider.GetRequiredService<IMessageObservable>();
                
        //        AddSerializationComponent(bufferContext.TransmittingBuffer, observable);
        //        AddDeserializationComponent(bufferContext.ReceivingBuffer, messageSender);
        //    });
        //}

        //private static void AddSerializationComponent(IBuffer buffer, IMessageObservable observable)
        //{
        //    var serializer = new SerializationComponent(buffer);
        //    observable.Subscribe<Transmit<Connect>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<ConnAck>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<Disconnect>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<Subscribe>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<SubAck>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<Publish>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<PubAck>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<PubRec>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<PubRel>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<PubComp>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<PingReq>>(serializer.HandleAsync);
        //    observable.Subscribe<Transmit<PingResp>>(serializer.HandleAsync);
        //}

        private static void AddDeserializationComponent(IBuffer buffer, IMessageSender messageSender)
        {
            //var deserializer = new DeserializationComponent(messageSender);
            //buffer.FlushRequested += deserializer.ProcessBufferFlushAsync;
        }
    }
}
