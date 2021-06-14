using HyperMsg.Mqtt.Packets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class ConnectionTests : IntegrationTestBase
    {
        [Fact]
        public async Task TransmitConnectionRequestAsync_Receives_ConAck_Response()
        {
            var conAckResponse = default(ConnAck);
            var responseResult = default(ConnectionResult?);
            var isSessionPresent = default(bool?);
            HandlersRegistry.RegisterMessageReceivedEventHandler<ConnAck>(conAck => conAckResponse = conAck);
            
            await StartConnectionListener();
            var task = MessagingContext.ConnectAsync(ConnectionSettings);
            
            task.Completion.Wait(DefaultWaitTimeout);

            Assert.NotNull(conAckResponse);
            Assert.Equal(responseResult, conAckResponse.ResultCode);
            Assert.Equal(isSessionPresent, conAckResponse.SessionPresent);

            await MessageSender.SendTransmitMessageCommandAsync(Disconnect.Instance, default);
        }

        [Fact]
        public async Task ConnectAsync_With_MqttClient()
        {
            await StartConnectionListener();
            var client = GetService<IMqttClient>();
            var options = GetService<IMqttClientOptions>();

            var result = await client.ConnectAsync(options);

            Assert.NotNull(result);

            await client.DisconnectAsync();
        }
    }
}
