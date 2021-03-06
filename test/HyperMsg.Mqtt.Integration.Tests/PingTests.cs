﻿using HyperMsg.Mqtt;
using HyperMsg.Mqtt.Packets;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class PingTests : IntegrationTestBase
    {
        [Fact]
        public async Task PingAsync_Receives_Ping_Response()
        {
            var pingResp = default(PingResp);
            HandlersRegistry.RegisterMessageReceivedEventHandler<PingResp>(r => pingResp = r);
            await StartConnectionListener();
            await ConnectAsync();

            var pingTask = MessagingContext.PingAsync();

            await pingTask.Completion;//.Wait(DefaultWaitTimeout);

            Assert.True(pingTask.Completion.IsCompleted);
            Assert.NotNull(pingResp);
        }

        [Fact]
        public async Task PingAsync_With_MqttClient()
        {
            await StartConnectionListener();
            var client = GetService<IMqttClient>();
            var options = GetService<IMqttClientOptions>();

            await client.ConnectAsync(options);
            await client.PingAsync(default);            

            await client.DisconnectAsync();
        }
    }
}
