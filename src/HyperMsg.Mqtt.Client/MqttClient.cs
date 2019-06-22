using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClient : IMqttClient, IMessageHandler<Packet>
    {
        private readonly IMessageSender<Packet> messageSender;
        private readonly MqttConnectionSettings connectionSettings;

        private readonly ConnectHandler connectHandler;
        private readonly PingHandler pingHandler;
        private readonly PublishHandler publishHandler;
        private readonly SubscriptionHandler subscriptionHandler;        

        public MqttClient(IMessageSender<Packet> messageSender, MqttConnectionSettings connectionSettings)
        {
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            this.connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
            connectHandler = new ConnectHandler(messageSender, connectionSettings);
            pingHandler = new PingHandler(messageSender);
            publishHandler = new PublishHandler(messageSender, OnPublishReceived);
            subscriptionHandler = new SubscriptionHandler(messageSender);
        }

        public async Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken cancellationToken = default)
        {
            //await handler.HandleAsync(TransportOperations.OpenConnection, cancellationToken);

            //if (connectionSettings.UseTls)
            //{
            //    await handler.HandleAsync(TransportOperations.SetTransportLevelSecurity, cancellationToken);
            //}
            
            return await connectHandler.SendConnectAsync(cleanSession, cancellationToken);
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await messageSender.SendAsync(Disconnect.Instance, cancellationToken);
            //await handler.HandleAsync(TransportOperations.CloseConnection);
        }

        public Task PingAsync(CancellationToken cancellationToken = default) => pingHandler.SendPingReqAsync(cancellationToken);

        public Task PublishAsync(PublishRequest request, CancellationToken cancellationToken = default)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            return publishHandler.SendPublishAsync(request, cancellationToken);
        }

        public Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default)
        {
            _ = requests ?? throw new ArgumentNullException(nameof(requests));
            return subscriptionHandler.SendSubscribeAsync(requests, cancellationToken);
        }

        public Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken cancellationToken = default)
        {
            _ = topics ?? throw new ArgumentNullException(nameof(topics));
            return subscriptionHandler.SendUnsubscribeAsync(topics, cancellationToken);
        }

        public void Handle(Packet packet)
        {
            switch (packet)
            {
                case ConnAck connAck:
                    connectHandler.OnConnAckReceived(connAck);
                    break;

                case SubAck subAck:
                    subscriptionHandler.OnSubAckReceived(subAck);
                    break;

                case UnsubAck unsubAck:
                    subscriptionHandler.OnUnsubAckReceived(unsubAck);
                    break;

                case PubAck pubAck:
                    publishHandler.OnPubAckReceived(pubAck);
                    break;

                case PubRec pubRec:
                    publishHandler.OnPubRecReceived(pubRec);
                    break;

                case PubComp pubComp:
                    publishHandler.OnPubCompReceived(pubComp);
                    break;

                case Publish publish:
                    publishHandler.OnPublishReceived(publish);
                    break;

                case PubRel pubRel:
                    publishHandler.OnPubRelReceived(pubRel);
                    break;

                case PingResp _:
                    pingHandler.OnPingRespReceived();
                    break;
            }
        }

        public Task HandleAsync(Packet message, CancellationToken cancellationToken = default)
        {
            Handle(message);
            return Task.CompletedTask;
        }

        private void OnPublishReceived(PublishReceivedEventArgs args) => PublishReceived?.Invoke(this, args);        

        public event EventHandler<PublishReceivedEventArgs> PublishReceived;
    }
}
