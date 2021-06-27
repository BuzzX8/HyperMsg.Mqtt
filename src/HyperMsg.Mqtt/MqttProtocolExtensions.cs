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
        #region Connection

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

        #endregion

        #region Subscription

        public static ushort SendSubscriptionRequest(this IMessageSender messageSender, IEnumerable<SubscriptionRequest> requests)
        {
            var request = CreateSubscribeRequest(requests);

            messageSender.SendToTransmitPipe(request);
            return request.Id;
        }

        public static async Task<ushort> SendSubscriptionRequestAsync(this IMessageSender messageSender, IEnumerable<SubscriptionRequest> requests, CancellationToken cancellationToken = default)
        {
            var request = CreateSubscribeRequest(requests);

            await messageSender.SendToTransmitPipeAsync(request, cancellationToken);
            return request.Id;
        }

        private static Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests) => new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));

        public static IDisposable RegisterSubscriptionResponseHandler(this IMessageHandlersRegistry handlersRegistry, Action<IReadOnlyList<(string topic, SubscriptionResult result)>> handler) =>
            handlersRegistry.RegisterReceivePipeHandler(typeof(SubAck), handler);

        public static IDisposable RegisterSubscriptionResponseHandler(this IMessageHandlersRegistry handlersRegistry, AsyncAction<IReadOnlyList<(string topic, SubscriptionResult result)>> handler) =>
            handlersRegistry.RegisterReceivePipeHandler(typeof(SubAck), handler);

        public static ushort SendUnsubscribeRequest(this IMessageSender messageSender, IEnumerable<string> topics)
        {
            var packet = new Unsubscribe(PacketId.New(), topics);

            messageSender.SendToTransmitPipe(packet);
            return packet.Id;
        }

        public static async Task<ushort> SendUnsubscribeRequestAsync(this IMessageSender messageSender, IEnumerable<string> topics, CancellationToken cancellationToken = default)
        {
            var packet = new Unsubscribe(PacketId.New(), topics);

            await messageSender.SendToTransmitPipeAsync(packet, cancellationToken);
            return packet.Id;
        }

        public static IDisposable RegisterUnsubscribeResponseHandler(this IMessageHandlersRegistry handlersRegistry, Action<IReadOnlyList<string>> handler) =>
            handlersRegistry.RegisterReceivePipeHandler(typeof(UnsubAck), handler);

        #endregion

        #region Publish

        public static ushort SendPublishRequest(this IMessageSender messageSender, string topic, ReadOnlyMemory<byte> message, QosLevel qos)
        {
            var publish = new Publish(PacketId.New(), topic, message, qos);
            messageSender.SendToTransmitPipe(publish);
            return publish.Id;
        }

        public static async Task<ushort> SendPublishRequestAsync(this IMessageSender messageSender, string topic, ReadOnlyMemory<byte> message, QosLevel qos, CancellationToken cancellationToken = default)
        {
            var publish = new Publish(PacketId.New(), topic, message, qos);
            await messageSender.SendToTransmitPipeAsync(publish);
            return publish.Id;
        }

        public static IDisposable RegisterPublishCompletedHandler(this IMessageHandlersRegistry handlersRegistry, Action<PublishCompletedHandlerArgs> handler) =>
            handlersRegistry.RegisterReceivePipeHandler(handler);

        public static IDisposable RegisterPublishCompletedHandler(this IMessageHandlersRegistry handlersRegistry, AsyncAction<PublishCompletedHandlerArgs> handler) =>
            handlersRegistry.RegisterReceivePipeHandler(handler);

        #endregion
    }

    public class PublishCompletedHandlerArgs
    {
        internal PublishCompletedHandlerArgs(ushort id, string topic, QosLevel qos) =>
            (Id, Topic, Qos) = (id, topic, qos);

        public ushort Id { get; }

        public string Topic { get; }

        public QosLevel Qos { get; }
    }
}
