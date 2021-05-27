using HyperMsg.Mqtt.Packets;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Extensions
{
    public static class MessageSenderExtensions
    {
        public static Task TransmitConnectionRequestAsync(this IMessageSender messageSender, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken = default)
        {
            var connectPacket = CreateConnectPacket(connectionSettings);
            return messageSender.SendTransmitMessageCommandAsync(connectPacket, cancellationToken);
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
