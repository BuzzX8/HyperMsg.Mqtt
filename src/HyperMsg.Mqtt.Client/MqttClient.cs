using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClient : IMqttClient
    {
        private readonly ConnectionComponent connectionComponent;
        private readonly PingComponent pingComponent;
        private readonly PublishComponent publishComponent;
        private readonly SubscriptionComponent subscriptionComponent;        

        public MqttClient(IMessageSender messageSender, MqttConnectionSettings connectionSettings)
        {
            connectionComponent = new ConnectionComponent(messageSender, connectionSettings);
            pingComponent = new PingComponent(messageSender);
            publishComponent = new PublishComponent(messageSender);
            subscriptionComponent = new SubscriptionComponent(messageSender);
        }

        public Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken cancellationToken = default) => connectionComponent.ConnectAsync(cleanSession, cancellationToken);

        public Task DisconnectAsync(CancellationToken cancellationToken = default) => connectionComponent.DisconnectAsync(cancellationToken);

        public Task PingAsync(CancellationToken cancellationToken = default) => pingComponent.PingAsync(cancellationToken);

        public Task PublishAsync(PublishRequest request, CancellationToken cancellationToken = default)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            return publishComponent.PublishAsync(request, cancellationToken);
        }

        public Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default)
        {
            _ = requests ?? throw new ArgumentNullException(nameof(requests));
            return subscriptionComponent.SubscribeAsync(requests, cancellationToken);
        }

        public Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken cancellationToken = default)
        {
            _ = topics ?? throw new ArgumentNullException(nameof(topics));
            return subscriptionComponent.UnsubscribeAsync(topics, cancellationToken);
        }

        internal void Handle(Received<ConnAck> connAck) => connectionComponent.Handle(connAck);

        internal void Handle(Received<SubAck> subAck) => subscriptionComponent.Handle(subAck);

        internal void Handle(Received<UnsubAck> unsubAck) => subscriptionComponent.Handle(unsubAck);

        internal Task HandleAsync(Received<Publish> publish, CancellationToken cancellationToken) => publishComponent.HandleAsync(publish, cancellationToken);

        internal void Handle(Received<PubAck> pubAck) => publishComponent.Handle(pubAck);

        internal Task HandleAsync(Received<PubRec> pubRec, CancellationToken cancellationToken) => publishComponent.HandleAsync(pubRec, cancellationToken);

        internal Task HandleAsync(Received<PubRel> pubRel, CancellationToken cancellationToken) => publishComponent.HandleAsync(pubRel, cancellationToken);

        internal void Handle(Received<PubComp> pubComp) => publishComponent.Handle(pubComp);

        internal void Handle(Received<PingResp> pingResp) => pingComponent.Handle(pingResp);

        public event EventHandler<PublishReceivedEventArgs> PublishReceived
        {
            add => publishComponent.PublishReceived += value;
            
            remove => publishComponent.PublishReceived -= value;
        }
    }
}
