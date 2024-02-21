using ReceiverService;
using System.Linq;
using DbProviders;
using RemoteProviders;
using Microsoft.Extensions.Configuration.Json;
using ReceiverService.Dtos;
using Newtonsoft.Json.Serialization;

var builder = Host.CreateApplicationBuilder(args);

IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();
WorkerOptions settings = config.GetRequiredSection("RabbitMQ").Get<WorkerOptions>();

builder.Services.AddSingleton(settings);
builder.Services.AddHostedService<Worker>();
var host = builder.Build();
host.Run();

