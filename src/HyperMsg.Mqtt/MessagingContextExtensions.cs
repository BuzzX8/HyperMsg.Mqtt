using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    public static class MessagingContextExtensions
    {
        public static Task<MessagingTask<SessionState>> ConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken = default)
        {
            return new ConnectTask(messagingContext, connectionSettings).StartAsync(cancellationToken);
        }
    }
}
