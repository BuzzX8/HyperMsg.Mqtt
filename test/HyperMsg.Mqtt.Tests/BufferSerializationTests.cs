using HyperMsg.Mqtt.Packets;
using System;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class BufferSerializationTests : HostFixture
    {
        public BufferSerializationTests() : base(services => services.AddMqttSerialization())
        { }

        [Fact]
        public void Serializes_Mqtt_Messages_Into_Transmitting_Buffer()
        {
            var expectedMessage = new Connect
            {
                ClientId = Guid.NewGuid().ToString()                
            };
            var actualMessage = default(object);

            SenderRegistry.Register<BufferUpdatedEvent>(@event =>
            {
                var reader = @event.Buffer.Reader;
                var data = reader.Read();
                var message = MqttDeserializer.Deserialize(data, out var bytesConsumed);
                actualMessage = message;
                reader.Advance(bytesConsumed);
            });

            Sender.Dispatch(expectedMessage);

            Assert.NotNull(actualMessage);
            Assert.Equal(expectedMessage, actualMessage);
        }
    }
}
