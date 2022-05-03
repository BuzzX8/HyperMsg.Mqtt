using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class ProtocolService : IHostedService
    {
        private readonly IContext context;
        private readonly RequestStorage requestStorage;

        public ProtocolService(IContext context) => (this.context, requestStorage) = (context, new());

        public Task StartAsync(CancellationToken cancellationToken)
        {
            RegisterSenderHandlers(context.Sender.Registry);
            RegisterReceiverHandlers(context.Receiver.Registry);

            return Task.CompletedTask;
        }

        private void RegisterSenderHandlers(IRegistry registry)
        {
            registry.Register<Subscribe>(HandleSubscribeRequest);
            registry.Register<Unsubscribe>(HandleUnsubscribeRequest);
            registry.Register<Publish>(HandlePublishRequest);
        }

        private void RegisterReceiverHandlers(IRegistry registry)
        {
            registry.Register<SubAck>(HandleSubAckResponse);
            registry.Register<UnsubAck>(HandleUnsubAckResponse);
            registry.Register<PubAck>(HandlePubAckResponseAsync);
            registry.Register<PubRec>(HandlePubRecResponseAsync);
            registry.Register<PubComp>(HandlePubCompResponseAsync);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            DeregisterSenderHandlers(context.Sender.Registry);
            DeregisterReceiverHandlers(context.Receiver.Registry);

            return Task.CompletedTask;
        }

        private void DeregisterSenderHandlers(IRegistry registry)
        {
            registry.Deregister<Subscribe>(HandleSubscribeRequest);
            registry.Deregister<Unsubscribe>(HandleUnsubscribeRequest);
            registry.Deregister<Publish>(HandlePublishRequest);
        }

        private void DeregisterReceiverHandlers(IRegistry registry)
        {
            registry.Deregister<SubAck>(HandleSubAckResponse);
            registry.Deregister<UnsubAck>(HandleUnsubAckResponse);
            registry.Deregister<PubAck>(HandlePubAckResponseAsync);
            registry.Deregister<PubRec>(HandlePubRecResponseAsync);
            registry.Deregister<PubComp>(HandlePubCompResponseAsync);
        }

        private void HandleSubscribeRequest(Subscribe subscribe) => requestStorage.AddOrReplace(subscribe.Id, subscribe);

        private void HandleSubAckResponse(SubAck subAck)
        {
            if (!requestStorage.TryGet<Subscribe>(subAck.Id, out var request))
            {
                return;
            }

            var requestedTopics = request.Subscriptions.Select(s => s.Item1).ToArray();
            context.Sender.Dispatch(new SubscriptionResponseHandlerArgs(requestedTopics, subAck.Results.ToArray()));

            requestStorage.Remove<Subscribe>(subAck.Id);
        }

        private void HandleUnsubscribeRequest(Unsubscribe unsubscribe) => requestStorage.AddOrReplace(unsubscribe.Id, unsubscribe);

        private void HandleUnsubAckResponse(UnsubAck unsubAck)
        {
            if (!requestStorage.TryGet<Unsubscribe>(unsubAck.Id, out var request))
            {
                return;
            }
            
            requestStorage.Remove<Unsubscribe>(unsubAck.Id);
        }

        private void HandlePublishRequest(Publish publish)
        {
            if (publish.Qos == QosLevel.Qos0)
            {
                return;
            }

            requestStorage.AddOrReplace(publish.Id, publish);
        }

        private void HandlePubAckResponseAsync(PubAck pubAck)
        {
            if (!requestStorage.TryGet<Publish>(pubAck.Id, out var publish) && publish.Qos != QosLevel.Qos1)
            {
                return;
            }

            context.Receiver.Dispatch(new PublishCompletedHandlerArgs(publish.Id, publish.Topic, publish.Qos));
            requestStorage.Remove<Publish>(publish.Id);
        }

        private void HandlePubRecResponseAsync(PubRec pubRec)
        {
            if (!requestStorage.TryGet<Publish>(pubRec.Id, out var publish) && publish.Qos != QosLevel.Qos2)
            {
                return;
            }

            context.Sender.Dispatch(new PubRel(pubRec.Id));
            requestStorage.AddOrReplace(pubRec.Id, pubRec);
        }

        private void HandlePubCompResponseAsync(PubComp pubComp)
        {
            if (!requestStorage.Contains<PubRec>(pubComp.Id) && !requestStorage.Contains<Publish>(pubComp.Id))
            {
                return;
            }

            var publish = requestStorage.Get<Publish>(pubComp.Id);

            if (publish.Qos != QosLevel.Qos2)
            {
                return;
            }

            context.Receiver.Dispatch(new PublishCompletedHandlerArgs(publish.Id, publish.Topic, publish.Qos));
            requestStorage.Remove<Publish>(publish.Id);
            requestStorage.Remove<PubRec>(publish.Id);
        }
    }
}
