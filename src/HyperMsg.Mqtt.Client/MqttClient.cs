using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClient : IMqttClient
    {        
        private readonly IMessageSender<Packet> messageSender;

        private readonly ConnectionComponent connectionController;
        private readonly PingComponent pingHandler;
        private readonly PublishComponent publishHandler;
        private readonly SubscriptionComponent subscriptionHandler;        

        public MqttClient(AsyncAction<TransportCommand> transportCommandHandler, 
                          IMessageSender<Packet> messageSender, 
                          MqttConnectionSettings connectionSettings)
        {
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));

            connectionController = new ConnectionComponent(transportCommandHandler, messageSender, connectionSettings);
            pingHandler = new PingComponent(messageSender);
            publishHandler = new PublishComponent(messageSender);
            subscriptionHandler = new SubscriptionComponent(messageSender);
        }

        public Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken cancellationToken = default) => connectionController.ConnectAsync(cleanSession, cancellationToken);

        public Task DisconnectAsync(CancellationToken cancellationToken = default) => connectionController.DisconnectAsync(cancellationToken);

        public Task PingAsync(CancellationToken cancellationToken = default) => pingHandler.PingAsync(cancellationToken);

        public Task PublishAsync(PublishRequest request, CancellationToken cancellationToken = default)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            return publishHandler.PublishAsync(request, cancellationToken);
        }

        public Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default)
        {
            _ = requests ?? throw new ArgumentNullException(nameof(requests));
            return subscriptionHandler.SubscribeAsync(requests, cancellationToken);
        }

        public Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken cancellationToken = default)
        {
            _ = topics ?? throw new ArgumentNullException(nameof(topics));
            return subscriptionHandler.UnsubscribeAsync(topics, cancellationToken);
        }
        public Task HandleAsync(Packet message, CancellationToken cancellationToken = default)
        {
            switch (message)
            {
                case ConnAck connAck:
                    connectionController.Handle(connAck);
                    break;

                case SubAck subAck:
                    subscriptionHandler.Handle(subAck);
                    break;

                case UnsubAck unsubAck:
                    subscriptionHandler.Handle(unsubAck);
                    break;

                case PubAck pubAck:
                    publishHandler.Handle(pubAck);
                    break;

                case PubRec pubRec:
                    return publishHandler.HandleAsync(pubRec, cancellationToken);

                case PubComp pubComp:
                    publishHandler.Handle(pubComp);
                    break;

                case Publish publish:
                    return publishHandler.HandleAsync(publish, cancellationToken);

                case PubRel pubRel:
                    return publishHandler.HandleAsync(pubRel, cancellationToken);

                case PingResp pingResp:
                    pingHandler.Handle(pingResp);
                    break;
            }

            return Task.CompletedTask;
        }

        public event EventHandler<PublishReceivedEventArgs> PublishReceived
        {
            add => publishHandler.PublishReceived += value;
            
            remove => publishHandler.PublishReceived -= value;
        }
    }
}
