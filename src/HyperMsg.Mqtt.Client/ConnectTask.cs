using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectTask
    {        
        private readonly CancellationToken cancellationToken;        
        private readonly IDisposable conAckSubscription;
        private readonly IDisposable cancelSubscription;
        private readonly TaskCompletionSource<SessionState> completionSource;

        internal ConnectTask(IMessageObservable messageObservable, CancellationToken cancellationToken)
        {
            completionSource = new TaskCompletionSource<SessionState>();
            conAckSubscription = messageObservable.OnReceived<ConnAck>(OnConAckReceived);
            cancelSubscription = cancellationToken.Register(Dispose);
        }

        internal Task<SessionState> Task => completionSource.Task;

        private void OnConAckReceived(ConnAck connAck)
        {
            var result = connAck.SessionPresent ? SessionState.Present : SessionState.Clean;
            completionSource.SetResult(result);
            conAckSubscription.Dispose();
        }

        public void Dispose()
        {
            conAckSubscription.Dispose();
            cancelSubscription.Dispose();
        }
    }
}
