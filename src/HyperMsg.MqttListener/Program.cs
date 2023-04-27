using HyperMsg.MqttListener;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((h, b) =>
    {
        //h.Configuration.
    })
    .ConfigureServices((context, services) =>
    {        
        services.AddHostedService<ListenerWorker>();
        services.Configure<ListeningOptions>(context.Configuration.GetSection(nameof(ListeningOptions)));
        //services.ConfigureOptions<ListeningOptions>();
    });
    
var host = builder.Build();

host.Run();
