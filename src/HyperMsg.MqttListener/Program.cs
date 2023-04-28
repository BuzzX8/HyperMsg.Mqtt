using HyperMsg.MqttListener;
using HyperMsg.MqttListener.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {        
        services.AddHostedService<ConnectionListener>();
        services.AddSingleton<IConnectionHandler, ConnectionHandler>();
        services.Configure<ListeningOptions>(context.Configuration.GetSection(nameof(ListeningOptions)));
    });
    
var host = builder.Build();

host.Run();
