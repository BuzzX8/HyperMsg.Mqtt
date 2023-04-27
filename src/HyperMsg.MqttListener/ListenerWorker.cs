namespace HyperMsg.MqttListener;

using System.Net.Sockets;

public class ListenerWorker : BackgroundService
{
    private readonly ILogger<ListenerWorker> _logger;
    private readonly Socket listeningSocket;

    public ListenerWorker(ILogger<ListenerWorker> logger)
    {
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
