using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class SubscriptionService : MessagingService
    {
        public SubscriptionService(IMessagingContext messagingContext) : base(messagingContext)
        {
        }

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<Subscribe>(HandleSubscribeRequest);
            yield return this.RegisterMessageReceivedEventHandler<Unsubscribe>(HandleUnsubscribeRequest);
        }

        private async Task HandleSubscribeRequest(Subscribe subscribe, CancellationToken cancellationToken)
        {
            var response = new SubAck(subscribe.Id, subscribe.Subscriptions.Select(s => SubscriptionResult.SuccessQos1));
            await this.SendTransmitMessageCommandAsync(response, cancellationToken);
        }

        private async Task HandleUnsubscribeRequest(Unsubscribe unsubscribe, CancellationToken cancellationToken)
        {

        }
    }
}
