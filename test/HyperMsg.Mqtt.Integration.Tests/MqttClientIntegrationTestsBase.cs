using HyperMsg.Mqtt.Client;
using HyperMsg.Mqtt.Serialization;
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

        private readonly ServiceContainer serviceProvider;

        public MqttClientIntegrationTestsBase()
        {
            ConnectionSettings = new MqttConnectionSettings(ClientId);
            serviceProvider = new ServiceContainer();
            serviceProvider.AddCoreServices(DefaultBufferSize, DefaultBufferSize);
            serviceProvider.AddSocketTransport(EndPoint);
            serviceProvider.AddMqttSerialization();
            serviceProvider.AddMqttClient(ConnectionSettings);
        }

        protected T GetService<T>() where T : class => serviceProvider.GetRequiredService<T>();

        protected IMqttClient Client => GetService<IMqttClient>();

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected Task<SessionState> ConnectAsync(bool cleanSession, CancellationToken cancellationToken) => Client.ConnectAsync(cleanSession, cancellationToken);

        protected Task DisconnectAsync(CancellationToken cancellationToken) => Client.DisconnectAsync(cancellationToken);

        public void Dispose() => DisconnectAsync(default).Wait();
    }
}