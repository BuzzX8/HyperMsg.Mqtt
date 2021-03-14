using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using HyperMsg.Sockets;
using HyperMsg.Sockets.Extensions;
using HyperMsg.Transport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class MqttConnectionService : MessagingService
    {
        private readonly IBufferFactory bufferFactory;
        private SocketDataTransferService dataTransferService;

        public MqttConnectionService(IBufferFactory bufferFactory, IMessagingContext messagingContext) : base(messagingContext)
        {
            this.bufferFactory = bufferFactory;            
        }

        protected override IEnumerable<IDisposable> GetDefaultDisposables()
        {
            yield return this.RegisterAcceptedSocketHandler(socket =>
            {
                var buffer = bufferFactory.CreateBuffer(-1);
                dataTransferService = new SocketDataTransferService(socket, buffer, MessagingContext);
                dataTransferService.StartAsync(default).Wait();
                Send(ConnectionEvent.Opened);
                return true;
            });
            yield return this.RegisterReceiveHandler<Connect>(connect =>
            {
                var conAck = new ConnAck(ConnectionResult.Accepted);
                this.Transmit(conAck);
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            dataTransferService?.Dispose();
        }
    }
}
