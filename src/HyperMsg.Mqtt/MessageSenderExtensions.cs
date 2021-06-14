﻿using HyperMsg.Mqtt.Packets;
using HyperMsg.Transport;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MessageSenderExtensions
    {
        public static async Task SendConnectionRequestAsync(this IMessageSender messageSender, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            await messageSender.SendConnectCommandAsync(cancellationToken);

            if (connectionSettings.UseTls)
            {
                await messageSender.SendSetTlsCommandAsync();
            }

            var connectPacket = CreateConnectPacket(connectionSettings);
            await messageSender.SendToTransmitBufferAsync(connectPacket, cancellationToken);
        }

        private static Connect CreateConnectPacket(MqttConnectionSettings connectionSettings)
        {
            var flags = ConnectFlags.None;

            if (connectionSettings.CleanSession)
            {
                flags |= ConnectFlags.CleanSession;
            }

            var connect = new Connect
            {
                ClientId = connectionSettings.ClientId,
                KeepAlive = connectionSettings.KeepAlive,
                Flags = flags
            };

            if (connectionSettings.WillMessageSettings != null)
            {
                connect.Flags |= ConnectFlags.Will;
                connect.WillTopic = connectionSettings.WillMessageSettings.Topic;
                connect.WillMessage = connectionSettings.WillMessageSettings.Message;
            }

            return connect;
        }

        public static Task SendSubscriptionRequestAsync(this IMessageSender messageSender, IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken)
        {
            var request = CreateSubscribeRequest(requests);
            //packetId = request.Id;

            return messageSender.SendToTransmitPipeAsync(request, cancellationToken);
        }

        private static Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests) => new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));
    }
}