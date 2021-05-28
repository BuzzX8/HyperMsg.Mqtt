using HyperMsg.Mqtt.Packets;
using System;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class BufferSerializationTests : ServiceHostFixture
    {
        public BufferSerializationTests() : base(services => services.AddMqttServices())
        { }

        [Fact]
        public void Serializes_Mqtt_Messages_Into_Transmitting_Buffer()
        {
            var expectedMessage = new Connect
            {
                ClientId = Guid.NewGuid().ToString()                
            };
            var actualMessage = default(object);

            HandlersRegistry.RegisterBufferFlushReader(BufferType.Transmitting, data =>
            {
                var message = MqttDeserializer.Deserialize(data, out var bytesConsumed);
                actualMessage = message;
                return bytesConsumed;
            });

            MessageSender.SendTransmitMessageCommand(expectedMessage);

            Assert.NotNull(actualMessage);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void Sends_Event_For_Received_Mqtt_Message()
        {
            var expectedMessage = new Connect
            {
                ClientId = Guid.NewGuid().ToString()
            };
            var actualMessage = default(object);
            HandlersRegistry.RegisterMessageReceivedEventHandler<Connect>(c => actualMessage = c);

            MessageSender.SendWriteToBufferCommand(BufferType.Receiving, expectedMessage);

            Assert.NotNull(actualMessage);
            Assert.Equal(expectedMessage, actualMessage);
        }
    }
}
