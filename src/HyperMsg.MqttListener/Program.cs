using HyperMsg.MqttListener;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {        
        services.AddHostedService<ConnectionListener>();
        services.Configure<ListeningOptions>(context.Configuration.GetSection(nameof(ListeningOptions)));
    });
    
var host = builder.Build();

host.Run();
