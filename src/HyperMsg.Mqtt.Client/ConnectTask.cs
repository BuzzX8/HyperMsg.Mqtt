using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class ConnectTask : IDisposable
    {
        private readonly IDisposable conAckSubscription;
        private IDisposable cancelSubscription;
        private readonly TaskCompletionSource<SessionState> completionSource;

        internal ConnectTask(IMessageObservable messageObservable)
        {
            completionSource = new TaskCompletionSource<SessionState>();
            conAckSubscription = messageObservable.OnReceived<ConnAck>(OnConAckReceived);            
        }

        public Task<SessionState> Completion => completionSource.Task;

        public TaskAwaiter<SessionState> GetAwaiter() => completionSource.Task.GetAwaiter();

        internal async Task RunAsync(IMessageSender messageSender, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            await messageSender.TransmitConnectAsync(connectionSettings, cancellationToken);
            cancelSubscription = cancellationToken.Register(Dispose);
        }

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
