using HyperMsg.Integration;
using HyperMsg.Mqtt.Client;
using HyperMsg.Mqtt.Serialization;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    public class ConnectionComponentTests : SocketTransportFixtureBase<Packet>
    {
        private readonly ConnectionComponent connectionComponent;
        private readonly MqttConnectionSettings connectionSettings;

        public ConnectionComponentTests() : base(1883)
        {
            connectionSettings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            connectionComponent = new ConnectionComponent(Transport.ProcessCommandAsync, MessageSender, connectionSettings);
            HandlerRegistry.Register(new Action<Packet>(p => connectionComponent.Handle(p as ConnAck)));
        }

        [Fact]
        public async Task ConnectAsync_Establishes_Connection()
        {
            var sessionState = await connectionComponent.ConnectAsync();

            Assert.Equal(SessionState.Present, sessionState);
        }

        protected override void ConfigureSerializer(IConfigurable configurable) => configurable.UseMqttSerializer();
    }
}
