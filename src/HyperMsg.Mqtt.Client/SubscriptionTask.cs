using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class SubscriptionTask : IDisposable
    {
        private readonly IDisposable subscription;
        private readonly TaskCompletionSource<IEnumerable<SubscriptionResult>> tsc;
        private readonly ushort packetId;

        private IDisposable cancelSubscription;

        internal SubscriptionTask(IMessageObservable messageObservable)
        {
            tsc = new TaskCompletionSource<IEnumerable<SubscriptionResult>>();
            subscription = messageObservable.OnReceived<SubAck>(Handle);
            packetId = PacketId.New();
        }

        public Task<IEnumerable<SubscriptionResult>> Completion => tsc.Task;

        public TaskAwaiter<IEnumerable<SubscriptionResult>> GetAwaiter() => tsc.Task.GetAwaiter();

        internal async Task RunAsync(IMessageSender messageSender, IEnumerable<(string, QosLevel)> requests, CancellationToken cancellationToken)
        {
            await messageSender.TransmitSubscribeAsync(packetId, requests, cancellationToken);
            cancelSubscription = cancellationToken.Register(Dispose);
        }

        private void Handle(SubAck subAck)
        {
            if (subAck.Id != packetId)
            {
                return;
            }

            tsc.SetResult(subAck.Results);
        }

        public void Dispose()
        {
            subscription.Dispose();
            cancelSubscription.Dispose();
        }
    }
}
