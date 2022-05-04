using HyperMsg.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HyperMsg.Mqtt
{
    public static class MqttProtocolExtensions
    {
        #region Connection

        public static void SendConnectionRequest(this IForwarder forwarder, MqttConnectionSettings connectionSettings)
        {
            var connectPacket = CreateConnectPacket(connectionSettings);
            forwarder.Dispatch(connectPacket);
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

        public static void SendDisconnectRequest(this IForwarder forwarder) => forwarder.Dispatch(Disconnect.Instance);

        #endregion

        #region Subscription

        public static ushort SendSubscriptionRequest(this IForwarder forwarder, IEnumerable<SubscriptionRequest> requests)
        {
            var request = CreateSubscribeRequest(requests);

            forwarder.Dispatch(request);
            return request.Id;
        }        

        private static Subscribe CreateSubscribeRequest(IEnumerable<SubscriptionRequest> requests) => new Subscribe(PacketId.New(), requests.Select(r => (r.TopicName, r.Qos)));

        public static ushort SendUnsubscribeRequest(this IForwarder forwarder, IEnumerable<string> topics)
        {
            var packet = new Unsubscribe(PacketId.New(), topics);

            forwarder.Dispatch(packet);
            return packet.Id;
        }

        #endregion

        #region Publish

        public static ushort SendPublishRequest(this IForwarder forwarder, string topic, ReadOnlyMemory<byte> message, QosLevel qos)
        {
            var publish = new Publish(PacketId.New(), topic, message, qos);
            forwarder.Dispatch(publish);
            return publish.Id;
        }

        #endregion

        public static void SendPingRequest(this IForwarder forwarder) => forwarder.Dispatch(PingReq.Instance);
    }

    public class SubscriptionResponseHandlerArgs
    {
        internal SubscriptionResponseHandlerArgs(IReadOnlyList<string> requestedTopics, IReadOnlyList<SubscriptionResult> subscriptionResults) =>
            (RequestedTopics, SubscriptionResults) = (requestedTopics, subscriptionResults);

        public IReadOnlyList<string> RequestedTopics { get; }

        public IReadOnlyList<SubscriptionResult> SubscriptionResults { get; }
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
