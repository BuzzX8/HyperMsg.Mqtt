using HyperMsg.Extensions;
using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;

namespace HyperMsg.Mqtt.Integration.Tests
{
    public class PingService : MessagingService
    {
        public PingService(IMessagingContext messagingContext) : base(messagingContext)
        {
        }

        protected override IEnumerable<IDisposable> GetDefaultDisposables()
        {
            yield return this.RegisterMessageReceivedEventHandler<PingReq>(_ =>
            {
                this.SendTransmitMessageCommand(PingResp.Instance);
            });
        }
    }
}
