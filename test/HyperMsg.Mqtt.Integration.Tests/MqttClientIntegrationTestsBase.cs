using HyperMsg.Integration;
using HyperMsg.Mqtt.Client;
using HyperMsg.Mqtt.Serialization;
using HyperMsg.Transport.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    [Collection("Integration")]
    public abstract class MqttClientIntegrationTestsBase : IntegrationFixtureBase, IDisposable
    {
        const int MqttPort = 1883;

        protected readonly string ClientId = Guid.NewGuid().ToString();
        private readonly List<Packet> responses;

        public MqttClientIntegrationTestsBase()
        {
            ConnectionSettings = new MqttConnectionSettings(ClientId);
            responses = new List<Packet>();
            Configurable.UseCoreServices(2048, 2048);
            Configurable.UseSockets(new IPEndPoint(IPAddress.Loopback, MqttPort));
            Configurable.UseMqttSerialization();
            Configurable.UseMqttClient(ConnectionSettings);
            HandlerRegistry.Register<Received<Packet>>(p => responses.Add(p.Message));
        }

        protected IMqttClient Client => GetService<IMqttClient>();

        protected IReadOnlyList<Packet> Responses => responses;

        protected Packet LastResponse => responses.LastOrDefault();

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected Task<SessionState> ConnectAsync(bool cleanSession, CancellationToken cancellationToken) => Client.ConnectAsync(cleanSession, cancellationToken);

        protected Task DisconnectAsync(CancellationToken cancellationToken) => Client.DisconnectAsync(cancellationToken);

        public void Dispose() => DisconnectAsync(default).Wait();

        protected override void ConfigureSerializer(IConfigurable configurable)
        {
            
        }

        protected override void ConfigureTransport(IConfigurable configurable)
        {
            
        }
    }
}