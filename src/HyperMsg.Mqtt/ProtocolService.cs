using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class ProtocolService : MessagingService
    {
        private readonly IDataRepository dataRepository;

        public ProtocolService(IMessagingContext messagingContext, IDataRepository dataRepository) : base(messagingContext) =>
            this.dataRepository = dataRepository;

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterTransportMessageHandler(TransportMessage.Opened, HandleOpeningTransportMessageAsync);

            yield return this.RegisterTransmitPipeHandler<Subscribe>(HandleSubscribeRequest);
            yield return this.RegisterTransmitPipeHandler<Unsubscribe>(HandleUnsubscribeRequest);
            yield return this.RegisterTransmitPipeHandler<Publish>(HandlePublishRequest);

            yield return this.RegisterReceivePipeHandler<SubAck>(HandleSubAckResponse);
            yield return this.RegisterReceivePipeHandler<UnsubAck>(HandleUnsubAckResponse);
            yield return this.RegisterReceivePipeHandler<PubAck>(HandlePubAckResponseAsync);
            yield return this.RegisterReceivePipeHandler<PubRec>(HandlePubRecResponseAsync);
            yield return this.RegisterReceivePipeHandler<PubComp>(HandlePubCompResponseAsync);
        }

        private async Task HandleOpeningTransportMessageAsync(CancellationToken cancellationToken)
        {
            if (!dataRepository.TryGet<MqttConnectionSettings>(out var settings))
            {
                return;
            }

            if (settings.UseTls)
            {
                await this.SendTransportMessageAsync(TransportMessage.SetTls, cancellationToken);
            }

            await this.SendConnectionRequestAsync(settings, cancellationToken);
        }

        private void HandleSubscribeRequest(Subscribe subscribe) => dataRepository.AddOrReplace(subscribe.Id, subscribe);

        private async Task HandleSubAckResponse(SubAck subAck, CancellationToken cancellationToken)
        {
            if (!dataRepository.TryGet<Subscribe>(subAck.Id, out var request))
            {
                return;
            }

            var topics = request.Subscriptions.Select(s => s.Item1).ToArray();
            var topicResponses = subAck.Results.ToArray();
            var result = new (string, SubscriptionResult)[topicResponses.Length];

            for(int i = 0; i < topicResponses.Length; i++)
            {
                result[i] = (topics[i], topicResponses[i]);
            }

            await this.SendToReceivePipeAsync<IReadOnlyList<(string, SubscriptionResult)>>(typeof(SubAck), result);

            dataRepository.Remove<Subscribe>(subAck.Id);
        }

        private void HandleUnsubscribeRequest(Unsubscribe unsubscribe) => dataRepository.AddOrReplace(unsubscribe.Id, unsubscribe);

        private async Task HandleUnsubAckResponse(UnsubAck unsubAck, CancellationToken cancellationToken)
        {
            if (!dataRepository.TryGet<Unsubscribe>(unsubAck.Id, out var request))
            {
                return;
            }

            await this.SendToReceivePipeAsync<IReadOnlyList<string>>(typeof(UnsubAck), request.Topics.ToArray(), cancellationToken);
            dataRepository.Remove<Unsubscribe>(unsubAck.Id);
        }

        private void HandlePublishRequest(Publish publish)
        {
            if (publish.Qos == QosLevel.Qos0)
            {
                return;
            }

            dataRepository.AddOrReplace(publish.Id, publish);
        }

        private async Task HandlePubAckResponseAsync(PubAck pubAck, CancellationToken cancellationToken)
        {
            if (!dataRepository.TryGet<Publish>(pubAck.Id, out var publish) && publish.Qos != QosLevel.Qos1)
            {
                return;
            }

            await this.SendToReceivePipeAsync(new PublishCompletedHandlerArgs(publish.Id, publish.Topic, publish.Qos), cancellationToken);
            dataRepository.Remove<Publish>(publish.Id);
        }

        private async Task HandlePubRecResponseAsync(PubRec pubRec, CancellationToken cancellationToken)
        {
            if (!dataRepository.TryGet<Publish>(pubRec.Id, out var publish) && publish.Qos != QosLevel.Qos2)
            {
                return;
            }

            await this.SendToTransmitPipeAsync(new PubRel(pubRec.Id), cancellationToken);
            dataRepository.AddOrReplace(pubRec.Id, pubRec);
        }

        private async Task HandlePubCompResponseAsync(PubComp pubComp, CancellationToken cancellationToken)
        {            
            if (!dataRepository.Contains<PubRec>(pubComp.Id) && !dataRepository.Contains<Publish>(pubComp.Id))
            {
                return;
            }

            var publish = dataRepository.Get<Publish>(pubComp.Id);

            if (publish.Qos != QosLevel.Qos2)
            {
                return;
            }

            await this.SendToReceivePipeAsync(new PublishCompletedHandlerArgs(publish.Id, publish.Topic, publish.Qos), cancellationToken);
            dataRepository.Remove<Publish>(publish.Id);
            dataRepository.Remove<PubRec>(publish.Id);
        }
    }
}
