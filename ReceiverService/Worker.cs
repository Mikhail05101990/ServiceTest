using DbProviders;
using Microsoft.Extensions.Options;
using RemoteProviders;
using ReceiverService.Dtos;

namespace ReceiverService;

public class Worker : BackgroundService
{
    static bool isRunning = true;
    private readonly ILogger<Worker> _logger;
    private readonly WorkerOptions _options;

    public Worker(ILogger<Worker> logger, WorkerOptions options)
    {
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var db = new StatusContext();
        
        Console.CancelKeyPress += delegate {
            isRunning = false;
        };

        RabbitMQSubscriber subscriber = new RabbitMQSubscriber(db, _options);

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
