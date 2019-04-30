using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public interface IMqttClient
    {
        Task<SessionState> ConnectAsync(bool cleanSession = false, CancellationToken token = default);

        Task DisconnectAsync(CancellationToken token = default);

        Task PublishAsync(PublishRequest request, CancellationToken token = default);
    }
}