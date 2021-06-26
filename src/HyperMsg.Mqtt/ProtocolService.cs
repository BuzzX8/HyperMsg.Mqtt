using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Collections.Generic;
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

        private void HandleUnsubscribeRequest(Unsubscribe unsubscribe) => dataRepository.AddOrReplace(unsubscribe.Id, unsubscribe);
    }
}
