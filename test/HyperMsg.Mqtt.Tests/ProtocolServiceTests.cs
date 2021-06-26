using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System.Linq;
using System.Threading.Tasks;
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
            MessageSender.SendTransportMessage(TransportMessage.Opened);

            Assert.NotNull(sentPacket);
            Assert.Equal(connectionSettings.ClientId, sentPacket.ClientId);
        }

        [Fact]
        public void Sends_SetTsl_If_UseTls_Setting_True()
        {
            var connectionSettings = new MqttConnectionSettings("test-client")
            {
                UseTls = true
            };
            dataRepository.AddOrReplace(connectionSettings);
            var isMessageSend = false;

            HandlersRegistry.RegisterTransportMessageHandler(TransportMessage.SetTls, () => isMessageSend = true);
            MessageSender.SendTransportMessage(TransportMessage.Opened);

            Assert.True(isMessageSend);
        }

        [Fact]
        public async Task SubscribeAsync_Sends_Correct_Subscribe_Request()
        {
            var subscribePacket = default(Subscribe);
            HandlersRegistry.RegisterTransmitPipeHandler<Subscribe>(subscribe => subscribePacket = subscribe);
            var request = Enumerable.Range(1, 5)
                .Select(i => new SubscriptionRequest($"topic-{i}", (QosLevel)(i % 3)))
                .ToArray();

            var packetId = await MessageSender.SendSubscriptionRequestAsync(request);

            Assert.NotNull(subscribePacket);
            Assert.Equal(packetId, subscribePacket.Id);
            Assert.True(dataRepository.Contains<Subscribe>(packetId));
        }

        [Fact]
        public async Task UnsubscribeAsync_Sends_Unsubscription_Request()
        {
            var unsubscribe = default(Unsubscribe);
            HandlersRegistry.RegisterTransmitPipeHandler<Unsubscribe>(packet => unsubscribe = packet);
            var topics = new[] { "topic-1", "topic-2" };

            var packetId = await MessageSender.SendUnsubscribeRequestAsync(topics);

            Assert.NotNull(unsubscribe);
            Assert.Equal(packetId, unsubscribe.Id);
            Assert.Equal(topics, unsubscribe.Topics);
            Assert.True(dataRepository.Contains<Unsubscribe>(unsubscribe.Id));
        }
    }
}
