﻿using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public static class MessageSenderExtensions
    {
        public static Task SendConnectRequestAsync(this IMessageSender messageSender, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
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
    }
}
