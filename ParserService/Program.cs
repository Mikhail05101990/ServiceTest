using CloudProviders.AMPQ;
using ServiceTest;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTransient<AMPQProvider>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
