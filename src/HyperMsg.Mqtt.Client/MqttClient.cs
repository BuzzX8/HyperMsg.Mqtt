using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClient : IMqttClient, IHandler<Packet>
    {
        private readonly ISender<Packet> sender;
        
        private readonly ConnectHandler connectHandler;
        private readonly PingHandler pingHandler;
        private readonly PublishHandler publishHandler;
        private readonly SubscriptionHandler subscriptionHandler;

        public MqttClient(ISender<Packet> sender, MqttConnectionSettings connectionSettings)
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            connectHandler = new ConnectHandler(sender, connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings)));
            pingHandler = new PingHandler(sender);
            publishHandler = new PublishHandler(sender, OnPublishReceived);
            subscriptionHandler = new SubscriptionHandler(sender);
        }

        public SessionState Connect(bool cleanSession = false) => ConnectAsync(cleanSession).GetAwaiter().GetResult();

        public async Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default)
        {
            if (SubmitTransportCommandAsync != null)
            {
                await SubmitTransportCommandAsync(TransportCommands.OpenConnection, token);
            }

            return await connectHandler.SendConnectAsync(cleanSession, token);
        }

        public void Disconnect() => DisconnectAsync().GetAwaiter().GetResult();

        public async Task DisconnectAsync(CancellationToken token = default)
        {
            if (SubmitTransportCommandAsync != null)
            {
                await SubmitTransportCommandAsync(TransportCommands.CloseConnection, token);
            }

            await sender.SendAsync(Mqtt.Disconnect.Instance, token);
        }

        public void Ping() => PingAsync().GetAwaiter().GetResult();

        public Task PingAsync(CancellationToken token = default) => pingHandler.SendPingReqAsync(token);

        public void Publish(PublishRequest request) => PublishAsync(request).GetAwaiter().GetResult();

        public Task PublishAsync(PublishRequest request, CancellationToken token = default)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            return publishHandler.SendPublishAsync(request, token);
        }

        public IEnumerable<SubscriptionResult> Subscribe(IEnumerable<SubscriptionRequest> requests) => SubscribeAsync(requests).GetAwaiter().GetResult();

        public Task<IEnumerable<SubscriptionResult>> SubscribeAsync(IEnumerable<SubscriptionRequest> requests, CancellationToken token = default)
        {
            _ = requests ?? throw new ArgumentNullException(nameof(requests));
            return subscriptionHandler.SendSubscribeAsync(requests, token);
        }

        public void Unsubscribe(IEnumerable<string> topics) => UnsubscribeAsync(topics).GetAwaiter().GetResult();

        public Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken token = default)
        {
            _ = topics ?? throw new ArgumentNullException(nameof(topics));
            return subscriptionHandler.SendUnsubscribeAsync(topics, token);
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

        public Task HandleAsync(Packet message, CancellationToken token = default)
        {
            Handle(message);
            return Task.CompletedTask;
        }

        private void OnPublishReceived(PublishReceivedEventArgs args) => PublishReceived?.Invoke(this, args);        

        public event EventHandler<PublishReceivedEventArgs> PublishReceived;

        public Func<ReceiveMode, CancellationToken, Task> SetReceiveModeAsync;

        public Func<TransportCommands, CancellationToken, Task> SubmitTransportCommandAsync;
    }
}
