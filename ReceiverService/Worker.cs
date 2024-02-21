using DbProviders;
using RemoteProviders;

namespace ReceiverService;

public class Worker : BackgroundService
{
    static bool isRunning = true;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var db = new StatusContext();
        
        Console.CancelKeyPress += delegate {
            isRunning = false;
        };

        RabbitMQSubscriber subscriber = new RabbitMQSubscriber(db);

        while (isRunning && !stoppingToken.IsCancellationRequested)
        {
            subscriber.ConsumeQueue();
            Thread.Sleep(1000);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Listener service is stopping");
        
        return;
    }
}
