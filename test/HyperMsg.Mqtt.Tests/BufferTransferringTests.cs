using HyperMsg.Mqtt.Packets;
using HyperMsg.Mqtt.Serialization;
using System;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class BufferTransferringTests : ServiceHostFixture
    {
        public BufferTransferringTests() : base(services => services.AddMqttServices())
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
                (var bytesConsumed, var message) = MqttDeserializer.Deserialize(data);
                actualMessage = message;
                return bytesConsumed;
            });

            MessageSender.SendTransmitMessageCommand(expectedMessage);

            Assert.NotNull(actualMessage);
            Assert.Equal(expectedMessage, actualMessage);
        }
    }
}
