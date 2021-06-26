using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MqttProtocolExtensions
    {
        public static void SendConnectionRequest(this IMessageSender messageSender, MqttConnectionSettings connectionSettings)
        {
            var connectPacket = CreateConnectPacket(connectionSettings);
            messageSender.SendToTransmitPipe(connectPacket);
        }

        public static async Task SendConnectionRequestAsync(this IMessageSender messageSender, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken = default)
        {
            var connectPacket = CreateConnectPacket(connectionSettings);
            await messageSender.SendToTransmitPipeAsync(connectPacket, cancellationToken);
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

        public static IDisposable RegisterConnectionResultHandler(this IMessageHandlersRegistry handlersRegistry, Action<ConnAck> handler) =>
            handlersRegistry.RegisterReceivePipeHandler(handler);

        public static IDisposable RegisterConnectionResultHandler(this IMessageHandlersRegistry handlersRegistry, AsyncAction<ConnAck> handler) =>
            handlersRegistry.RegisterReceivePipeHandler(handler);

        public static async Task<ushort> SendSubscriptionRequestAsync(this IMessageSender messageSender, IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default)
        {
            var request = CreateSubscribeRequest(requests);

            await messageSender.SendToTransmitPipeAsync(request, cancellationToken);
            return request.Id;
        }

        private static Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests) => new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));

        public static async Task<ushort> SendUnsubscribeRequestAsync(this IMessageSender messageSender, IEnumerable<string> topics, CancellationToken cancellationToken = default)
        {
            var packet = new Unsubscribe(PacketId.New(), topics);

            await messageSender.SendToTransmitPipeAsync(packet, cancellationToken);
            return packet.Id;
        }
    }
}
