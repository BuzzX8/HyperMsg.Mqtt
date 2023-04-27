using HyperMsg.MqttListener;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ListenerWorker>();
    })
    .Build();

host.Run();
