using HyperMsg.Mqtt.Extensions;
using HyperMsg.Sockets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public abstract class IntegrationTestBase
    {
        protected static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(5);
        private readonly ServiceHost host;

        protected IntegrationTestBase()
        {
            host = ServiceHost.CreateDefault(services => services.AddMqttServices().AddLocalSocketConnection(1883));
            host.StartAsync().Wait();

            ConnectionSettings = new MqttConnectionSettings("HyperMsg");
            MessagingContext = host.GetRequiredService<IMessagingContext>();
        }

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected IMessagingContext MessagingContext { get; }

        protected IMessageSender MessageSender => MessagingContext.Sender;

        protected IMessageHandlersRegistry MessageObservable => MessagingContext.HandlersRegistry;

        protected T GetRequiredService<T>() => host.GetRequiredService<T>();

        protected async Task ConnectAsync() => await await MessagingContext.ConnectAsync(ConnectionSettings, default);
    }
}
