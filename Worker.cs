using CloudProviders.AMPQ;
using FileSystem.Parsers;

namespace ServiceTest;

public class Worker : BackgroundService
{
    readonly ILogger<Worker> _logger;
    int readDocsPeriodicity = 1000;
    string targetFolder = "Data";
        static bool isRunning = true;


    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.CancelKeyPress += delegate {
            isRunning = false;
        };

        try
        {
            AMPQProvider provider = new ();
            XmlParser parser = new XmlParser(targetFolder);
            while(isRunning)
            {
                try
                {
                    parser.ProcessFiles();
                }
                catch(Exception ex)
                {
                    _logger.LogError($"Error on process files: {ex.Message}");
                }
                
                Thread.Sleep(readDocsPeriodicity);

                provider.SendMessage("{\"id\": 13, \"name\": \"ben\"}");
            }
        }
        catch(Exception e)
        {
            _logger.LogError($"Error: {e.Message}");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service is Stopping");
        
        return;
    }
}
