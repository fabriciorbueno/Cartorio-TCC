using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace CartorioTcc.CrossCutting.Logging;

public static class SerilogExtensions
{
    public static IHostBuilder UseCartorioTccSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            ConfigureLogger(context.Configuration, loggerConfiguration);
        });
    }

    public static void ConfigureLogger(IConfiguration configuration, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "CartorioTcc.SAEC")
            .Enrich.WithProperty("Serventia", "2º Tabelionato de Notas de Campinas")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Application}] {Message:lj}{NewLine}{Exception}");
    }
}
