namespace HyperMsg.MqttListener.Services
{
    public interface IConnectionHandler
    {
        void HandleConnection(System.Net.Sockets.Socket connection, CancellationToken stoppingToken);
    }
}
