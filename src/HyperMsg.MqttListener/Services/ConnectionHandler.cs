using HyperMsg.Mqtt;
using HyperMsg.Mqtt.Coding;
using HyperMsg.Mqtt.Packets;
using System.Buffers;
using System.Collections.Immutable;

namespace HyperMsg.MqttListener.Services
{
    public class ConnectionHandler : IConnectionHandler, IDisposable
    {
        private readonly MemoryPool<byte> _memoryPool;
        private readonly ILogger<ConnectionHandler> _logger;

        public ConnectionHandler(ILogger<ConnectionHandler> logger)
        {
            _memoryPool = MemoryPool<byte>.Shared;
            _logger = logger;
        }

        public void HandleConnection(System.Net.Sockets.Socket connection, CancellationToken stoppingToken)
        {
            _ = Task.Run(() => HandleTask(connection, stoppingToken), stoppingToken).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _logger.LogError(task.Exception, null);
                }

                connection.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                connection.Disconnect(false);
                connection.Dispose();
            }, stoppingToken);
        }

        private async Task HandleTask(System.Net.Sockets.Socket connection, CancellationToken stoppingToken)
        {
            var receivingBuffer = _memoryPool.Rent(1000);

            while (!stoppingToken.IsCancellationRequested)
            {
                var received = await connection.ReceiveAsync(receivingBuffer.Memory, stoppingToken);
            }
        }

        public void Dispose()
        {
        }
    }
}
