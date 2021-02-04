using HyperMsg.Connection;
using HyperMsg.Extensions;
using HyperMsg.Mqtt.Extensions;
using HyperMsg.Mqtt.Packets;
using MQTTnet;
using MQTTnet.Client.Options;
using System.Threading;
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
            var responseResult = default(ConnectionResult?);
            var isSessionPresent = default(bool?);
            var @event = new ManualResetEventSlim();

            HandlersRegistry.RegisterReceiveHandler<ConnAck>(response => conAckResponse = response);
            HandlersRegistry.RegisterConnectionResponseReceiveHandler((result, clean) =>
            {
                responseResult = result;
                isSessionPresent = clean;
                @event.Set();
            });

            await MessageSender.SendAsync(ConnectionCommand.Open, default);
            await MessageSender.TransmitConnectionRequestAsync(ConnectionSettings);

            @event.Wait(DefaultWaitTimeout);

            Assert.True(@event.IsSet);
            Assert.NotNull(conAckResponse);
            Assert.Equal(responseResult, conAckResponse.ResultCode);
            Assert.Equal(isSessionPresent, conAckResponse.SessionPresent);
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
