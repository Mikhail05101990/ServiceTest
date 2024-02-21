using CloudProviders.AMPQ;
using FileSystem.Parsers;

namespace ServiceTest;

public class Worker : BackgroundService
{
    static bool isRunning = true;
    ILogger<Worker> _logger;
    int readDocsPeriodicity = 1000;
    readonly IServiceProvider _serviceProvider;
    string targetFolder = string.Join(AppDomain.CurrentDomain.BaseDirectory, "Data");
    
    public Worker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var wLogger = _serviceProvider.GetService(typeof(ILogger<Worker>)) as ILogger<Worker>;
        
        if(wLogger == null)
            throw new Exception("Failed to instantiate logger for Worker");

        _logger = wLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.CancelKeyPress += delegate {
            isRunning = false;
        };

        try
        {
            AMPQProvider provider = new ();
            
            var pLogger = _serviceProvider.GetService(typeof(ILogger<XmlParser>)) as ILogger<XmlParser>;
            
            if(pLogger == null)
                throw new Exception("Failed to instantiate logger for XmlParser");

            var rabbitProvider = _serviceProvider.GetService(typeof(AMPQProvider)) as AMPQProvider;
            
            if(rabbitProvider == null)
                throw new Exception("Failed to instantiate rabbitMq for XmlParser");

            XmlParser parser = new XmlParser(targetFolder, pLogger, rabbitProvider);
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
