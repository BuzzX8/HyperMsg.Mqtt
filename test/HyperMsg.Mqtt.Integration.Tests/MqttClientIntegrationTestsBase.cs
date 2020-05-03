using HyperMsg.Mqtt.Client;
using HyperMsg.Mqtt.Serialization;
using HyperMsg.Transport;
using HyperMsg.Transport.Sockets;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    [Collection("Integration")]
    public abstract class MqttClientIntegrationTestsBase : IDisposable
    {
        const int DefaultBufferSize = 2048;
        const int MqttPort = 1883;

        protected readonly string ClientId = Guid.NewGuid().ToString();
        protected readonly IPEndPoint EndPoint = new IPEndPoint(IPAddress.Loopback, MqttPort);

        private readonly ServiceContainer serviceContainer;

        public MqttClientIntegrationTestsBase()
        {
            ConnectionSettings = new MqttConnectionSettings(ClientId);
            serviceContainer = new ServiceContainer();
            serviceContainer.AddCoreServices(DefaultBufferSize, DefaultBufferSize);
            serviceContainer.AddSocketTransport(EndPoint);
            serviceContainer.AddMqttSerialization();
            serviceContainer.AddMqttClient(ConnectionSettings);
        }

        protected T GetService<T>() where T : class => serviceContainer.GetRequiredService<T>();

        protected IMessagingContext MessagingContext => GetService<IMessagingContext>();

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected async Task<SessionState> ConnectAsync(CancellationToken cancellationToken = default)
        {
            var context = GetService<IMessagingContext>();
            await context.Sender.SendAsync(TransportCommand.Open, cancellationToken);
            return await await context.StartConnectAsync(ConnectionSettings, cancellationToken);
        }

        protected async Task DisconnectAsync(CancellationToken cancellationToken) 
        { 
        }

        public void Dispose() => DisconnectAsync(default).Wait();
    }
}