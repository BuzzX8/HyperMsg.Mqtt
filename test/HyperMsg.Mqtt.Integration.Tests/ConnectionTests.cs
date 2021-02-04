using HyperMsg.Extensions;
using HyperMsg.Mqtt.Extensions;
using HyperMsg.Mqtt.Packets;
using MQTTnet;
using MQTTnet.Client.Options;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class ConnectionTests : IntegrationTestBase
    {
        [Fact]
        public async Task ConnectAsync_Receives_ConAck_Response()
        {            
            var conAckResponse = default(ConnAck);
            MessageObservable.RegisterReceiveHandler<ConnAck>(c => conAckResponse = c);

            var task = await MessagingContext.ConnectAsync(ConnectionSettings);

            task.AsTask().Wait(DefaultWaitTimeout);

            Assert.True(task.IsCompleted);
            Assert.NotNull(conAckResponse);
        }

        [Fact]
        public async Task DisconnectAsync_Closes_Connection()
        {
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId("HyperM-Client")
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
                .WithTcpServer("localhost")
                .WithCleanSession()
                .Build();

            //await mqttClient.ConnectAsync(options, default);
            //await mqttClient.DisconnectAsync(new MQTTnet.Client.Disconnecting.MqttClientDisconnectOptions(), default);

            //var port = GetRequiredService<IPort>();
            //await await MessagingContext.ConnectAsync(ConnectionSettings);
            //Assert.True(port.IsOpen);
            //await MessagingContext.DisconnectAsync();

            //Assert.False(port.IsOpen);
        }
    }
}
