using HyperMsg.Mqtt.Extensions;
using HyperMsg.Sockets.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client.Options;
using System;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public abstract class IntegrationTestBase : ServiceHostFixture
    {
        protected static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(5);

        const string hostName = "localhost";
        const int port = 1883;

        protected IntegrationTestBase() : base(services =>
        {
            services.AddMqttServices()
                .AddSocketConnection(hostName, port)
                .AddSingleton(provider =>
                {
                    var factory = new MqttFactory();
                    return factory.CreateMqttClient();
                })
                .AddSingleton(provider =>
                {
                    return new MqttClientOptionsBuilder()
                        .WithClientId("HyperM-Client")
                        .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
                        .WithTcpServer(hostName)
                        .WithCleanSession()
                        .Build();
                });
        })
        {
            ConnectionSettings = new MqttConnectionSettings("HyperMsg");
        }

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected async Task ConnectAsync() => await await MessagingContext.ConnectAsync(ConnectionSettings, default);
    }
}
