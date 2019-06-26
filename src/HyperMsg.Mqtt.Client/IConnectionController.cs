using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Client
{
    public interface IConnectionController : IMessageHandler<ConnAck>
    {
        Task<SessionState> ConnectAsync(bool cleanSession, CancellationToken cancellationToken);

        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}
