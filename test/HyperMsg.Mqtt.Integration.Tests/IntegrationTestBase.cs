using HyperMsg.Extensions;
using HyperMsg.Mqtt.Extensions;
using HyperMsg.Sockets.Extensions;
using HyperMsg.Transport.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client.Options;
using System;
using System.Net;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public abstract class IntegrationTestBase : ServiceHostFixture
    {
        protected static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(5);

        const string hostName = "127.0.0.1";
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
            ServerHost = ServiceHost.CreateDefault(services => services.AddSocketConnectionListener(new IPEndPoint(IPAddress.Any, port))
                .AddBufferFactory()
                .AddMqttServices()
                .AddHostedService<MqttConnectionService>());
            ServerHost.Start();
        }

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected ServiceHost ServerHost { get; }

        protected async Task ConnectAsync() => await MessagingContext.ConnectAsync(ConnectionSettings, default);

        protected async Task StartConnectionListener()
        {
            var messageSender = ServerHost.GetRequiredService<IMessageSender>();
            await messageSender.SendStartConnectionListeningCommandAsync();
        }
    }
}
