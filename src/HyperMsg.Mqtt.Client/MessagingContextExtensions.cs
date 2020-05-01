using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public static class MessagingContextExtensions
    {
        public static async Task<SessionState> ConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            await messagingContext.Sender.TransmitConnectAsync(connectionSettings, cancellationToken);
            return await new ConnectTask(messagingContext.Observable, cancellationToken).Task;
        }
    }
}
