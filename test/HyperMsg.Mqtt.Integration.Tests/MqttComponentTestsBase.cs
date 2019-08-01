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
    public abstract class MqttComponentTestsBase : SocketTransportFixtureBase<Packet>, IDisposable
    {
        const int MqttPort = 1883;

        protected readonly string ClientId = Guid.NewGuid().ToString();
        private readonly ConnectionComponent connectionComponent;
        private readonly List<Packet> responses;

        public MqttComponentTestsBase() : base(MqttPort)
        {
            ConnectionSettings = new MqttConnectionSettings(ClientId);
            connectionComponent = new ConnectionComponent(Transport.ProcessCommandAsync, MessageSender, ConnectionSettings);
            responses = new List<Packet>();

            HandlerRegistry.Register(new Action<Packet>(p =>
            {
                if (p is ConnAck connAck)
                {
                    connectionComponent.Handle(connAck);
                }
            }));
            HandlerRegistry.Register(p => responses.Add(p));
        }

        protected IReadOnlyList<Packet> Responses => responses;

        protected Packet LastResponse => responses.LastOrDefault();

        protected MqttConnectionSettings ConnectionSettings { get; }

        protected override void ConfigureSerializer(IConfigurable configurable) => configurable.UseMqttSerializer();

        protected Task<SessionState> ConnectAsync(bool cleanSession, CancellationToken cancellationToken) => connectionComponent.ConnectAsync(cleanSession, cancellationToken);

        protected Task DisconnectAsync(CancellationToken cancellationToken) => connectionComponent.DisconnectAsync(cancellationToken);

        public void Dispose() => DisconnectAsync(default).Wait();
    }
}