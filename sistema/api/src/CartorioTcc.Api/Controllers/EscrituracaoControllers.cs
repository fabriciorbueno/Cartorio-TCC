using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CartorioTcc.Api.Authorization;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Core.Notifications;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Enums;
using CartorioTcc.Business.Interfaces;

namespace CartorioTcc.Api.Controllers;

/// <summary>
/// UC-06: Gerar Orçamento de Escrituras com base na Tabela de Custas CNB/SP
/// </summary>
[Route("api/orcamentos")]
[Authorize(Policy = CartorioPolicies.Atendimento)]
public sealed class OrcamentosController(
    INotificadorService notificador,
    IOrcamentoQueryService orcamentoQuery,
    IOrcamentoService orcamentoService,
    IPdfGeneratorService pdfGenerator) : BaseApiController(notificador)
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 50, CancellationToken ct = default)
        => CustomResponse(await orcamentoQuery.ListarRecentesAsync(limite, ct));

    [HttpGet("tabela-custas")]
    [AllowAnonymous]
    public IActionResult ObterTabelaCustas()
        => CustomResponse(orcamentoQuery.ListarFaixasTabelaCustas());

    [HttpPost("simular")]
    [AllowAnonymous]
    public IActionResult Simular([FromBody] CalcularOrcamentoRequest request)
        => CustomResponse(orcamentoService.Simular(request));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var dto = await orcamentoQuery.ObterPorIdAsync(id, ct);
        if (dto is null)
        {
            Notificador.NotificarNaoEncontrado("Orçamento não encontrado.");
            return CustomResponse();
        }
        return CustomResponse(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CalcularOrcamentoRequest request, CancellationToken ct)
        => CustomResponse(await orcamentoService.GerarESalvarAsync(request, ct));

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> BaixarPdf(int id, CancellationToken ct)
    {
        var dto = await orcamentoQuery.ObterPorIdAsync(id, ct);
        if (dto is null)
        {
            Notificador.NotificarNaoEncontrado("Orçamento não encontrado para emissão do PDF.");
            return CustomResponse();
        }
        return CustomFileResponse(pdfGenerator.GerarOrcamentoPdf(dto));
    }
}

/// <summary>
/// UC-07: Gerenciar Protocolos de Atendimento e UC-12: Consultar Andamento de Processo (Web / Mobile)
/// </summary>
[Route("api/protocolos")]
[Authorize(Policy = CartorioPolicies.Atendimento)]
public sealed class ProtocolosController(
    INotificadorService notificador,
    IProtocoloQueryService protocoloQuery,
    IProtocoloService protocoloService,
    IPdfGeneratorService pdfGenerator) : BaseApiController(notificador)
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] FiltroProtocoloDto filtro, CancellationToken ct)
        => CustomResponse(await protocoloQuery.PesquisarAsync(filtro, ct));

    [HttpGet("disponiveis-para-escritura")]
    public async Task<IActionResult> ListarDisponiveisParaEscritura([FromQuery] int? incluirProtocoloId, CancellationToken ct)
        => CustomResponse(await protocoloQuery.ListarProtocolosDisponiveisParaEscrituraAsync(incluirProtocoloId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var dto = await protocoloQuery.ObterPorIdAsync(id, ct);
        if (dto is null)
        {
            Notificador.NotificarNaoEncontrado("Protocolo não encontrado.");
            return CustomResponse();
        }
        return CustomResponse(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] SalvarProtocoloCommand command, CancellationToken ct)
        => CustomResponse(await protocoloService.IncluirAsync(command, ct));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Alterar(int id, [FromBody] SalvarProtocoloCommand command, CancellationToken ct)
        => CustomResponse(await protocoloService.AlterarAsync(id, command, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
        => CustomResponse(await protocoloService.InativarAsync(id, ct));

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> BaixarPdf(int id, CancellationToken ct)
    {
        var dto = await protocoloQuery.ObterPorIdAsync(id, ct);
        if (dto is null)
        {
            Notificador.NotificarNaoEncontrado("Protocolo não encontrado para emissão do PDF.");
            return CustomResponse();
        }
        return CustomFileResponse(pdfGenerator.GerarProtocoloAtendimentoPdf(dto));
    }

    /// <summary>
    /// UC-12: Consultar Andamento de Processo pelo Número de Protocolo (Área Pública / Mobile)
    /// </summary>
    [HttpGet("publico/{numeroProtocolo}")]
    [AllowAnonymous]
    public async Task<IActionResult> ConsultarAndamentoPublico(string numeroProtocolo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(numeroProtocolo))
        {
            Notificador.NotificarErro("Informe o número do protocolo para consultar o andamento.");
            return CustomResponse();
        }

        var andamento = await protocoloQuery.ConsultarAndamentoPublicoAsync(numeroProtocolo, ct);
        if (andamento is null)
        {
            Notificador.NotificarNaoEncontrado("Nenhum processo encontrado para o número de protocolo informado. Verifique os dígitos e tente novamente.");
            return CustomResponse();
        }

        return CustomResponse(andamento);
    }
}

/// <summary>
/// UC-09: Gerenciar Escrituras, Índice de Livros, Digitalizações PDF,
/// UC-10: Gerar Arquivo XML DOI (Receita Federal) e UC-11: Emitir Certidões em PDF.
/// </summary>
[Route("api/escrituras")]
[Authorize(Policy = CartorioPolicies.Escrituracao)]
public sealed class EscriturasController(
    INotificadorService notificador,
    IEscrituraQueryService escrituraQuery,
    IClienteQueryService clienteQuery,
    IEscrituraService escrituraService) : BaseApiController(notificador)
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] FiltroEscrituraDto filtro, CancellationToken ct)
        => CustomResponse(await escrituraQuery.PesquisarAsync(filtro, ct));

    [HttpGet("indice-livros")]
    public async Task<IActionResult> ConsultarIndiceLivros([FromQuery] FiltroEscrituraDto filtro, CancellationToken ct)
        => CustomResponse(await escrituraQuery.ConsultarIndiceAsync(filtro, ct));

    [HttpGet("documentos-digitalizados")]
    public async Task<IActionResult> PesquisarDocumentosDigitalizados([FromQuery] string? nomeOuCpf, CancellationToken ct)
        => CustomResponse(await clienteQuery.ConsultarDocumentosPorClienteAsync(nomeOuCpf ?? string.Empty, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var dto = await escrituraQuery.ObterPorIdAsync(id, ct);
        if (dto is null)
        {
            Notificador.NotificarNaoEncontrado("Escritura não encontrada.");
            return CustomResponse();
        }
        return CustomResponse(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] SalvarEscrituraCommand command, CancellationToken ct)
        => CustomResponse(await escrituraService.IncluirAsync(command, ct));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] SalvarEscrituraCommand command, CancellationToken ct)
        => CustomResponse(await escrituraService.AlterarAsync(id, command, ct));

    [HttpPost("{id:int}/finalizar-lavratura")]
    public async Task<IActionResult> FinalizarLavratura(int id, [FromBody] FinalizarEscrituraCommand? command, CancellationToken ct)
        => CustomResponse(await escrituraService.FinalizarLavraturaAsync(
            new FinalizarEscrituraCommand(id, command?.TextoMinutaDefinitivo), ct));

    [HttpPost("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id, [FromBody] InativarEscrituraCommand command, CancellationToken ct)
        => CustomResponse(await escrituraService.InativarAsync(command with { IdEscritura = id }, ct));

    [HttpPost("{id:int}/retirada")]
    public async Task<IActionResult> RegistrarRetirada(int id, [FromBody] RegistrarRetiradaProtocoloCommand command, CancellationToken ct)
        => CustomResponse(await escrituraService.RegistrarRetiradaDocumentosAsync(command with { IdEscritura = id }, ct));

    [HttpGet("{id:int}/protocolo-retirada-pdf")]
    public async Task<IActionResult> BaixarProtocoloRetiradaPdf(int id, CancellationToken ct)
        => CustomFileResponse(await escrituraService.GerarProtocoloRetiradaPdfAsync(id, ct));

    /// <summary>
    /// UC-11: Emitir Certidão da Escritura em PDF (por ID da escritura ou número de protocolo)
    /// </summary>
    [HttpGet("{id:int}/certidao-pdf")]
    public async Task<IActionResult> EmitirCertidaoPdf(int id, CancellationToken ct)
        => CustomFileResponse(await escrituraService.GerarCertidaoPdfAsync(id.ToString(), ct));

    [HttpGet("certidao-por-protocolo/{numeroProtocolo}")]
    public async Task<IActionResult> EmitirCertidaoPorProtocolo(string numeroProtocolo, CancellationToken ct)
        => CustomFileResponse(await escrituraService.GerarCertidaoPdfAsync(numeroProtocolo, ct));

    /// <summary>
    /// UC-10: Gerar arquivo XML para Envio DOI (Declaração sobre Operações Imobiliárias - Receita Federal)
    /// </summary>
    [HttpGet("{id:int}/exportar-doi-xml")]
    public async Task<IActionResult> ExportarDoiXml(int id, CancellationToken ct)
        => CustomFileResponse(await escrituraService.GerarArquivoDoiXmlAsync(id.ToString(), ct));

    [HttpGet("exportar-doi-por-protocolo/{numeroProtocolo}")]
    public async Task<IActionResult> ExportarDoiPorProtocolo(string numeroProtocolo, CancellationToken ct)
        => CustomFileResponse(await escrituraService.GerarArquivoDoiXmlAsync(numeroProtocolo, ct));

    [HttpPost("anexar-pdf-base64")]
    public async Task<IActionResult> AnexarPdfBase64([FromBody] UploadDocumentoBase64Request request, CancellationToken ct)
    {
        byte[] conteudo;
        try
        {
            var base64 = request.ConteudoBase64.Contains(',')
                ? request.ConteudoBase64.Split(',')[1]
                : request.ConteudoBase64;
            conteudo = Convert.FromBase64String(base64);
        }
        catch
        {
            Notificador.NotificarErro("Conteúdo Base64 do arquivo PDF é inválido.");
            return CustomResponse();
        }

        var doc = await escrituraService.AnexarDocumentoPdfAsync(
            new AnexarDocumentoCommand(
                request.EscrituraId,
                request.ClienteId,
                request.TipoDocumento,
                request.NomeArquivo,
                "application/pdf",
                conteudo,
                request.Descricao),
            ct);

        return CustomResponse(doc);
    }

    [HttpGet("documentos-digitalizados/{documentoId:int}/download")]
    public async Task<IActionResult> BaixarDocumentoDigitalizado(int documentoId, CancellationToken ct)
        => CustomFileResponse(await escrituraService.BaixarDocumentoPdfAsync(documentoId, ct));

    [HttpDelete("documentos-digitalizados/{documentoId:int}")]
    public async Task<IActionResult> RemoverDocumentoDigitalizado(int documentoId, CancellationToken ct)
        => CustomResponse(await escrituraService.RemoverDocumentoPdfAsync(documentoId, ct));
}

public sealed record UploadDocumentoBase64Request(
    int? EscrituraId,
    int? ClienteId,
    string NomeArquivo,
    string ConteudoBase64,
    TipoDocumentoDigitalizado TipoDocumento,
    string? Descricao);

/// <summary>
/// UC-03: Gerar Backup Criptografado Agendado e UC-05: Gerar Manualmente Backup Criptografado
/// </summary>
[Route("api/backups")]
[Authorize(Policy = CartorioPolicies.Administracao)]
public sealed class BackupsController(
    INotificadorService notificador,
    IRelatorioQueryService relatorioQuery,
    IBackupService backupService) : BaseApiController(notificador)
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => CustomResponse(await relatorioQuery.ListarBackupsAsync(ct));

    [HttpPost("manual")]
    public async Task<IActionResult> ExecutarBackupManual([FromBody] RealizarBackupManualCommand command, CancellationToken ct)
        => CustomResponse(await backupService.RealizarBackupManualAsync(command, ct));

    [HttpPost("agendado")]
    public async Task<IActionResult> ConfigurarBackupAgendado([FromBody] AgendarBackupCommand command, CancellationToken ct)
        => CustomResponse(await backupService.AgendarBackupAsync(command, ct));

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> BaixarBackupCriptografado(int id, CancellationToken ct)
        => CustomFileResponse(await backupService.BaixarBackupCriptografadoAsync(id, ct));
}

/// <summary>
/// UC-04: Gerar Relatórios Gerenciais (em tela e em PDF) e Dashboard Notarial
/// </summary>
[Route("api/relatorios")]
[Authorize(Policy = CartorioPolicies.Relatorios)]
public sealed class RelatoriosController(
    INotificadorService notificador,
    IRelatorioQueryService relatorioQuery,
    IPdfGeneratorService pdfGenerator,
    IUserContext userContext) : BaseApiController(notificador)
{
    [HttpGet("dashboard")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterDashboard(CancellationToken ct)
        => CustomResponse(await relatorioQuery.ObterDashboardResumoAsync(ct));

    [HttpPost("gerar")]
    public async Task<IActionResult> GerarRelatorioEmTela([FromBody] FiltroRelatorioRequest filtro, CancellationToken ct)
    {
        var resultado = await relatorioQuery.GerarRelatorioAsync(filtro, userContext.Nome, ct);
        return CustomResponse(resultado);
    }

    [HttpPost("gerar-pdf")]
    public async Task<IActionResult> GerarRelatorioPdf([FromBody] FiltroRelatorioRequest filtro, CancellationToken ct)
    {
        var resultado = await relatorioQuery.GerarRelatorioAsync(filtro, userContext.Nome, ct);
        var pdf = pdfGenerator.GerarRelatorioPdf(resultado);
        return CustomFileResponse(pdf);
    }
}
