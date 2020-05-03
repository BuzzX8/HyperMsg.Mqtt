using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public static class MessagingContextExtensions
    {
        public static async Task<ConnectTask> StartConnectAsync(this IMessagingContext messagingContext, MqttConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            var task = new ConnectTask(messagingContext.Observable);
            await task.RunAsync(messagingContext.Sender, connectionSettings, cancellationToken);
            return task;
        }
    }
}
