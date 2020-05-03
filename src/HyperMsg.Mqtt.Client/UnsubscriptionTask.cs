using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class UnsubscriptionTask : IDisposable
    {
        private readonly IDisposable subscription;
        private readonly TaskCompletionSource<bool> tsc;
        private readonly ushort packetId;
        private IDisposable cancelSubscription;

        public UnsubscriptionTask(IMessageObservable messageObservable)
        {
            subscription = messageObservable.OnReceived<UnsubAck>(Handle);
            tsc = new TaskCompletionSource<bool>();
            packetId = PacketId.New();
        }

        public Task Completion => tsc.Task;

        public TaskAwaiter GetAwaiter() => Completion.GetAwaiter();

        internal async Task RunAsync(IMessageSender messageSender, IEnumerable<string> topics, CancellationToken cancellationToken)
        {
            await messageSender.TransmitUnsubscribeAsync(packetId, topics, cancellationToken);
            cancelSubscription = cancellationToken.Register(Dispose);
        }

        private void Handle(UnsubAck unsubAck)
        {
            if (unsubAck.Id != packetId)
            {
                return;
            }

            tsc.SetResult(true);
        }

        public void Dispose()
        {
            subscription.Dispose();
            cancelSubscription.Dispose();
        }
    }
}
