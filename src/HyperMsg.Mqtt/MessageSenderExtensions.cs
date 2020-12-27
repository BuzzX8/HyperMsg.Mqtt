using HyperMsg.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MessageSenderExtensions
    {
        public static Task TransmitConnectAsync(this IMessageSender messageSender, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            var packet = CreateConnectPacket(connectionSettings);
            return messageSender.TransmitAsync(packet, cancellationToken);
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

        public static Task TransmitDisconnectAsync(this IMessageSender messageSender, CancellationToken cancellationToken)
        {
            return messageSender.TransmitAsync(Disconnect.Instance, cancellationToken);
        }

        public static Task TransmitSubscribeAsync(this IMessageSender messageSender, ushort packetId, IEnumerable<(string, QosLevel)> requests, CancellationToken cancellationToken)
        {
            var request = new Subscribe(packetId, requests);
            return messageSender.TransmitAsync(request, cancellationToken);
        }

        public static Task TransmitUnsubscribeAsync(this IMessageSender messageSender, ushort packetId, IEnumerable<string> topics, CancellationToken cancellationToken)
        {
            var request = new Unsubscribe(packetId, topics);
            return messageSender.TransmitAsync(request, cancellationToken);
        }

        public static Task TransmitPublishAsync(this IMessageSender messageSender, ushort packetId, string topic, QosLevel qosLevel, ReadOnlyMemory<byte> message, bool retain, bool dup, CancellationToken cancellationToken)
        {
            var publish = new Publish(packetId, topic, message, qosLevel)
            {
                Dup = dup,
                Retain = retain
            };
            return messageSender.TransmitAsync(publish, cancellationToken);
        }

        public static Task TransmitPingReqAsync(this IMessageSender messageSender, CancellationToken cancellationToken)
        {
            return messageSender.TransmitAsync(PingReq.Instance, cancellationToken);
        }
    }
}
