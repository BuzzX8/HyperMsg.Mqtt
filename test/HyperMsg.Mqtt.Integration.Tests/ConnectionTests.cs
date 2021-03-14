using HyperMsg.Transport;
using HyperMsg.Extensions;
using HyperMsg.Mqtt.Extensions;
using HyperMsg.Mqtt.Packets;
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
    }
}
