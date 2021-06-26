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

            yield return this.RegisterReceivePipeHandler<SubAck>(HandleSubAckResponse);
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

            await this.SendToReceivePipeAsync<IReadOnlyList<(string, SubscriptionResult)>>(result);

            dataRepository.Remove<Subscribe>(subAck.Id);
        }

        private void HandleUnsubscribeRequest(Unsubscribe unsubscribe) => dataRepository.AddOrReplace(unsubscribe.Id, unsubscribe);        
    }
}
