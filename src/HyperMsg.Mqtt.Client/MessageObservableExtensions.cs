using System;

namespace HyperMsg.Mqtt.Client
{
    public static class MessageObservableExtensions
    {
        public static void OnConnectResponse(this IMessageObservable messageObservable, Action<(ConnectionResult ResultCode, bool IsSessionPresent)> observer)
        {
            messageObservable.OnReceive<ConnAck>(m => observer.Invoke((m.ResultCode, m.SessionPresent)));
        }
    }
}