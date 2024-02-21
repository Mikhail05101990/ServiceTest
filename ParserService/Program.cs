using CloudProviders.AMPQ;
using ServiceTest.Dtos;
using ServiceTest;

var builder = Host.CreateApplicationBuilder(args);

IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();
WorkerOptions settings = config.GetRequiredSection("RabbitMQ").Get<WorkerOptions>();

builder.Services.AddSingleton(settings);
builder.Services.AddTransient<AMPQProvider>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
