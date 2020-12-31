using HyperMsg.Extensions;
using HyperMsg.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public abstract class IntegrationTestBase
    {
        protected static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(5);
        private readonly Host host;

        protected IntegrationTestBase()
        {
            var services = new ServiceCollection();
            ConnectionSettings = new MqttConnectionSettings("HyperMsg");
            services.AddMessagingServices()
                .AddMqttServices()
                .AddSocketTransport("localhost", 1883);
            host = new Host(services);
            host.StartAsync().Wait();

            MessagingContext = host.Services.GetRequiredService<IMessagingContext>();
        }

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected IMessagingContext MessagingContext { get; }

        protected IMessageSender MessageSender => MessagingContext.Sender;

        protected IMessageObservable MessageObservable => MessagingContext.Observable;

        protected T GetRequiredService<T>() => host.Services.GetRequiredService<T>();

        protected async Task ConnectAsync() => await await MessagingContext.ConnectAsync(ConnectionSettings, default);
    }
}
