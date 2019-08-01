using HyperMsg.Integration;
using HyperMsg.Mqtt.Client;
using HyperMsg.Mqtt.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HyperMsg.Mqtt.Integration
{
    [Collection("Integration")]
    public abstract class MqttClientIntegrationTestsBase : SocketTransportFixtureBase<Packet>, IDisposable
    {
        const int MqttPort = 1883;

        protected readonly string ClientId = Guid.NewGuid().ToString();
        private readonly List<Packet> responses;

        public MqttClientIntegrationTestsBase() : base(MqttPort)
        {
            ConnectionSettings = new MqttConnectionSettings(ClientId);
            responses = new List<Packet>();
            HandlerRegistry.Register(p => responses.Add(p));
        }

        protected IMqttClient Client => GetService<IMqttClient>();

        protected IReadOnlyList<Packet> Responses => responses;

        protected Packet LastResponse => responses.LastOrDefault();

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected override void ConfigureSerializer(IConfigurable configurable) => configurable.UseMqttSerializer();

        protected override void ConfigureServices(IConfigurable configurable)
        {
            base.ConfigureServices(configurable);
            configurable.UseMqttClient(ConnectionSettings);
        }

        protected Task<SessionState> ConnectAsync(bool cleanSession, CancellationToken cancellationToken) => Client.ConnectAsync(cleanSession, cancellationToken);

        protected Task DisconnectAsync(CancellationToken cancellationToken) => Client.DisconnectAsync(cancellationToken);

        public void Dispose() => DisconnectAsync(default).Wait();
    }
}