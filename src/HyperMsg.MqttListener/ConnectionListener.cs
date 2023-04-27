namespace HyperMsg.MqttListener;

using HyperMsg.Mqtt;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

public class ConnectionListener : BackgroundService
{
    private readonly ILogger<ConnectionListener> _logger;
    private readonly IOptions<ListeningOptions> _listeningOptions;
    private Socket listeningSocket;

    public ConnectionListener(ILogger<ConnectionListener> logger, IOptions<ListeningOptions> options)
    {
        _logger = logger;
        _listeningOptions = options;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {        
        var endpoint = IPEndPoint.Parse(_listeningOptions.Value.Endpoint);

        listeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listeningSocket.Bind(endpoint);
        listeningSocket.Listen();

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        //var received = listeningSocket.S;
        listeningSocket.Dispose();

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new byte[10000];

        while (!stoppingToken.IsCancellationRequested)
        {
            var acceptedSocket = await listeningSocket.AcceptAsync(stoppingToken);
            var received = await acceptedSocket.ReceiveAsync(buffer, stoppingToken);

            var packet = Decoding.Decode(buffer.AsMemory()[..received], out var consumed);
        }
    }
}
