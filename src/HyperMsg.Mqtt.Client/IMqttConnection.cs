using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public interface IMqttConnection
    {
        Task<SessionState> ConnectAsync(bool cleanSession, CancellationToken cancellationToken);

        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}