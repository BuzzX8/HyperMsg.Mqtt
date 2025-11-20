namespace HyperMsg.MqttListener;

using HyperMsg.MqttListener.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

public class ConnectionListener : BackgroundService
{
    private readonly IConnectionHandler _connectionHandler;
    private readonly ILogger<ConnectionListener> _logger;
    private readonly IOptions<ListeningOptions> _listeningOptions;

    private readonly Socket _listeningSocket;

    public ConnectionListener(IConnectionHandler connectionHandler, ILogger<ConnectionListener> logger, IOptions<ListeningOptions> options)
    {
        _connectionHandler = connectionHandler;
        _logger = logger;
        _listeningOptions = options;

        _listeningSocket = new(SocketType.Stream, ProtocolType.Tcp);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var endpoint = IPEndPoint.Parse(_listeningOptions.Value.Endpoint);

        _listeningSocket.Bind(endpoint);
        _listeningSocket.Listen();

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _listeningSocket.Dispose();

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var acceptedConnection = await _listeningSocket.AcceptAsync(stoppingToken);

            _connectionHandler.HandleConnection(acceptedConnection, stoppingToken);
        }
    }
}
