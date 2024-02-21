using ReceiverService;
using System.Linq;
using DbProviders;
using Microsoft.EntityFrameworkCore;
using RemoteProviders;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
