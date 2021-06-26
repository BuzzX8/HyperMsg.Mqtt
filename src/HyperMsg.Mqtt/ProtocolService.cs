﻿using HyperMsg.Transport;
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
            yield return this.RegisterTransportMessageHandler(TransportMessage.Opening, HandleOpeningTransportMessageAsync);
        }

        private async Task HandleOpeningTransportMessageAsync(CancellationToken cancellationToken)
        {
            if (!dataRepository.TryGet<MqttConnectionSettings>(out var settings))
            {
                return;
            }

            await this.SendConnectionRequestAsync(settings, cancellationToken);
        }
    }
}
