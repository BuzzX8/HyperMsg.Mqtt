using HyperMsg.Mqtt.Packets;
using System;

namespace HyperMsg.Mqtt.Extensions
{
    public static class MessageHandlersRegistryExtensions
    {
        public static IDisposable RegisterConnectionResponseReceiveHandler(this IMessageHandlersRegistry handlersRegistry, Action<ConnectionResult, bool> messageHandler)
            => handlersRegistry.RegisterMessageReceivedEventHandler<ConnAck>(response => messageHandler.Invoke(response.ResultCode, response.SessionPresent));

        public static IDisposable RegisterConnectionResponseReceiveHandler(this IMessageHandlersRegistry handlersRegistry, AsyncAction<ConnectionResult, bool> messageHandler)
            => handlersRegistry.RegisterMessageReceivedEventHandler<ConnAck>((response, token) => messageHandler.Invoke(response.ResultCode, response.SessionPresent, token));
    }
}
