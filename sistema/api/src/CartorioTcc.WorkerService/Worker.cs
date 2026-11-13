using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Enums;
using CartorioTcc.Business.Interfaces;

namespace CartorioTcc.WorkerService;

/// <summary>
/// Worker assíncrono em segundo plano responsável por:
/// 1. UC-03: Execução automática periódica dos Backups Criptografados agendados;
/// 2. UC-10: Monitoramento de escrituras lavradas pendentes de envio DOI à Receita Federal.
/// </summary>
public sealed class Worker(
    IServiceProvider serviceProvider,
    ILogger<Worker> logger,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CartorioTcc.WorkerService iniciado. Monitorando agendamentos de Backup Criptografado (UC-03) e fila DOI (UC-10).");

        var intervalSeconds = configuration.GetValue("Worker:IntervalSeconds", 3600);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                var escrituraQuery = scope.ServiceProvider.GetRequiredService<IEscrituraQueryService>();

                var processados = await backupService.ProcessarBackupsAgendadosAsync(stoppingToken);
                var escrituras = await escrituraQuery.PesquisarAsync(new FiltroEscrituraDto(), stoppingToken);
                var pendentesDoi = escrituras.Itens.Count(e => e.StatusEscritura == StatusEscritura.Finalizada && e.DataEnvioDoi is null);

                logger.LogInformation(
                    "Ciclo de processamento SAEC concluído. Backups agendados executados: {Processados}. Escrituras concluídas pendentes de exportação DOI: {PendentesDoi}.",
                    processados,
                    pendentesDoi);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro durante ciclo assíncrono do CartorioTcc.WorkerService.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}
