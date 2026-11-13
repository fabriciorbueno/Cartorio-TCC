using Serilog;
using CartorioTcc.CrossCutting.IoC;
using CartorioTcc.CrossCutting.Logging;
using CartorioTcc.WorkerService;

var builder = Host.CreateApplicationBuilder(args);

var loggerConfig = new LoggerConfiguration();
SerilogExtensions.ConfigureLogger(builder.Configuration, loggerConfig);
Log.Logger = loggerConfig.CreateLogger();

builder.Services.AddSerilog(Log.Logger);
builder.Services.RegisterCartorioTccServices(builder.Configuration, forceInMemoryDatabase: true);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
