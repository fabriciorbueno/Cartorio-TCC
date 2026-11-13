using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Core.Notifications;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Business.Services;
using CartorioTcc.Infra.Data.Context;
using CartorioTcc.Infra.Data.Queries;
using CartorioTcc.Infra.Data.Repositories;
using CartorioTcc.Infra.Documents;
using CartorioTcc.Infra.ExternalServices;

namespace CartorioTcc.CrossCutting.IoC;

public static class NativeInjectorBootStrapperExtensions
{
    public static IServiceCollection RegisterCartorioTccServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool forceInMemoryDatabase = false,
        string? inMemoryDbName = null)
    {
        // Contexto do Usuário (Mock enquanto Autenticação está em Bypass) e Data/Hora
        services.AddScoped<MockUserContext>();
        services.AddScoped<IUserContext>(sp => sp.GetRequiredService<MockUserContext>());
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Notification Pattern
        services.AddScoped<INotificadorService, NotificadorService>();

        // Persistência EF Core: SQL Server (Padrão) ou InMemory (Testes / Contingência)
        var useInMemory = forceInMemoryDatabase ||
                          bool.TryParse(configuration["Database:UseInMemory"], out var cfgInMem) && cfgInMem;

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=CartorioTccSaecDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<CartorioTccContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase(inMemoryDbName ?? "CartorioTccSaecInMemoryDb");
            }
            else
            {
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(CartorioTccContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(3);
                });
            }
        });

        // Repositories (Lado de Escrita - CQRS)
        services.AddScoped<IPermissaoRepository, PermissaoRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IOrcamentoRepository, OrcamentoRepository>();
        services.AddScoped<IProtocoloRepository, ProtocoloRepository>();
        services.AddScoped<IEscrituraRepository, EscrituraRepository>();
        services.AddScoped<IBackupRepository, BackupRepository>();

        // QueryServices (Lado de Leitura com AsNoTracking - CQRS)
        services.AddScoped<ICatalogoQueryService, CatalogoQueryService>();
        services.AddScoped<IFuncionarioQueryService, FuncionarioQueryService>();
        services.AddScoped<IClienteQueryService, ClienteQueryService>();
        services.AddScoped<IOrcamentoQueryService, OrcamentoQueryService>();
        services.AddScoped<IProtocoloQueryService, ProtocoloQueryService>();
        services.AddScoped<IEscrituraQueryService, EscrituraQueryService>();
        services.AddScoped<IRelatorioQueryService, RelatorioQueryService>();

        // Services de Domínio / Casos de Uso (Business)
        services.AddScoped<IPermissaoService, PermissaoService>();
        services.AddScoped<IFuncionarioService, FuncionarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IOrcamentoService, OrcamentoService>();
        services.AddScoped<IProtocoloService, ProtocoloService>();
        services.AddScoped<IEscrituraService, EscrituraService>();
        services.AddScoped<IBackupService, BackupService>();

        // Geradores de Documentos PDF, XML DOI e Criptografia de Backup
        services.AddScoped<IDoiExportGenerator, DoiExportGenerator>();
        services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
        services.AddScoped<IBackupCriptografadoProvider, BackupCriptografadoProvider>();

        // Clientes HTTP Externos Tipados via Refit (ViaCEP)
        var viaCepUrl = configuration["ExternalApis:ViaCepBaseUrl"] ?? "https://viacep.com.br";
        services.AddRefitClient<IViaCepRefitApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(viaCepUrl);
                c.Timeout = TimeSpan.FromSeconds(4);
            });
        services.AddScoped<ICepExternalService, CepExternalService>();

        return services;
    }
}
