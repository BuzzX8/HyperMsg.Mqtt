using FakeItEasy;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class MessagingContextExtensionsTests
    {
        [Fact]
        public async Task ConnectAsync_()
        {
            var context = A.Fake<IMessagingContext>();
            var settings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            //await context.ConnectAsync(settings, default);
        }
    }
}
