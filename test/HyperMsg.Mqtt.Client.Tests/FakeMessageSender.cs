using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public class FakeMessageSender : IMessageSender
    {
        private List<object> sentMessages = new List<object>();
        private ManualResetEventSlim syncEvent = new ManualResetEventSlim();

        public IReadOnlyList<object> SentMessages => sentMessages;

        public T GetLastMessage<T>() => (T)sentMessages.Last();

        public T GetLastTransmit<T>()
        {
            var lastTransmit = GetLastMessage<Transmit<T>>();
            return lastTransmit.Message;
        }

        public void WaitMessageToSent()
        {            
            syncEvent.Wait(TimeSpan.FromSeconds(10));
            syncEvent.Reset();
        }

        public void Send<T>(T message)
        {
            sentMessages.Add(message);
            syncEvent.Set();
        }

        public Task SendAsync<T>(T message, CancellationToken cancellationToken)
        {
            sentMessages.Add(message);
            syncEvent.Set();
            return Task.CompletedTask;
        }
    }
}
