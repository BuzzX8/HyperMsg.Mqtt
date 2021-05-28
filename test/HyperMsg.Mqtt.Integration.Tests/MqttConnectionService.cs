using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System;
using System.Collections.Generic;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class MqttConnectionService : MessagingService
    {
        public MqttConnectionService(IMessagingContext messagingContext) : base(messagingContext)
        {
        }

        protected override IEnumerable<IDisposable> GetAutoDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<Connect>(connect =>
            {
                var conAck = new ConnAck(ConnectionResult.Accepted);
                this.SendTransmitMessageCommand(conAck);
            });
            yield return this.RegisterMessageReceivedEventHandler<Disconnect>((_, token) =>
            {
                return this.SendCloseConnectionCommandAsync(token);
            });
        }        
    }
}
