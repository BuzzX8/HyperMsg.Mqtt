using HyperMsg.Extensions;
using HyperMsg.Mqtt.Extensions;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Sockets.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class MqttConnectionService : MessagingService
    {
        private IServiceScope acceptedSocketScope;

        public MqttConnectionService(IMessagingContext messagingContext) : base(messagingContext)
        {
        }

        protected override IEnumerable<IDisposable> GetDefaultDisposables()
        {
            yield return this.RegisterAcceptedSocketHandler(socket =>
            {
                acceptedSocketScope = this.CreateSocketScope(socket, services =>
                {
                    services.AddMqttServices();
                });

                return true;
            });
            yield return this.RegisterMessageReceivedEventHandler<Connect>(connect =>
            {
                var conAck = new ConnAck(ConnectionResult.Accepted);
                this.SendTransmitMessageCommand(conAck);
            });
        }        
    }
}
