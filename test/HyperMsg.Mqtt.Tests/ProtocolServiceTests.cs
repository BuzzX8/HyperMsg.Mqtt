using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class ProtocolServiceTests : ServiceHostFixture
    {
        private readonly IDataRepository dataRepository;

        public ProtocolServiceTests() : base(services => services.AddMqttServices()) =>
            dataRepository = GetRequiredService<IDataRepository>();

        [Fact]
        public void Sends_Connect_Packet_When_Receives_Opening_Transport_Message()
        {
            var connectionSettings = new MqttConnectionSettings("test-client");
            dataRepository.AddOrReplace(connectionSettings);
            var sentPacket = default(Connect);

            HandlersRegistry.RegisterTransmitPipeHandler<Connect>(packet => sentPacket = packet);
            MessageSender.SendTransportMessage(TransportMessage.Opening);

            Assert.NotNull(sentPacket);
            Assert.Equal(connectionSettings.ClientId, sentPacket.ClientId);
        }
    }
}
