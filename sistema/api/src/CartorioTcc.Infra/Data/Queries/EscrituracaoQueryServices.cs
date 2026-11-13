using CartorioTcc.Business.Builders;
using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Core.Validations;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Enums;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CartorioTcc.Infra.Data.Queries;

public sealed class OrcamentoQueryService(CartorioTccContext context) : IOrcamentoQueryService
{
    public async Task<IReadOnlyList<OrcamentoDto>> ListarRecentesAsync(int limite = 50, CancellationToken ct = default) =>
        await context.Orcamentos.AsNoTracking()
            .Include(o => o.Cliente)
            .OrderByDescending(o => o.DtCriacao)
            .Take(limite)
            .Select(o => new OrcamentoDto(
                o.Id,
                o.IdCliente,
                o.Cliente != null ? o.Cliente.NomeCliente : null,
                o.ValorBaseAto,
                o.Oneroso,
                o.Hipoteca,
                o.Ufesp2500,
                o.Salarios30,
                o.CertidaoInclusa,
                o.OutrosValores,
                o.FaixaDescricao,
                o.EmolumentosTabelionato,
                o.EmolumentosEstado,
                o.ContribuicaoIpesp,
                o.FundoRegistroCivil,
                o.TaxaTribunalJustica,
                o.FundoMinisterioPublico,
                o.IssqnEstimado,
                o.ValorCertidao,
                o.PrevisaoRegistroImoveis,
                o.ValorTotalOrcamento,
                o.DtCriacao))
            .ToListAsync(ct);

    public async Task<OrcamentoDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await context.Orcamentos.AsNoTracking()
            .Include(o => o.Cliente)
            .Where(o => o.Id == id)
            .Select(o => new OrcamentoDto(
                o.Id,
                o.IdCliente,
                o.Cliente != null ? o.Cliente.NomeCliente : null,
                o.ValorBaseAto,
                o.Oneroso,
                o.Hipoteca,
                o.Ufesp2500,
                o.Salarios30,
                o.CertidaoInclusa,
                o.OutrosValores,
                o.FaixaDescricao,
                o.EmolumentosTabelionato,
                o.EmolumentosEstado,
                o.ContribuicaoIpesp,
                o.FundoRegistroCivil,
                o.TaxaTribunalJustica,
                o.FundoMinisterioPublico,
                o.IssqnEstimado,
                o.ValorCertidao,
                o.PrevisaoRegistroImoveis,
                o.ValorTotalOrcamento,
                o.DtCriacao))
            .FirstOrDefaultAsync(ct);

    public IReadOnlyList<FaixaCustasDto> ListarFaixasTabelaCustas() =>
        TabelaCustasNotariaisCalculator.ObterTabelaFaixas();
}

public sealed class ProtocoloQueryService(CartorioTccContext context) : IProtocoloQueryService
{
    public async Task<ResultadoPaginado<ProtocoloDto>> PesquisarAsync(FiltroProtocoloDto filtro, CancellationToken ct = default)
    {
        var query = context.Protocolos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Ato)
            .Include(p => p.Funcionario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var t = filtro.Termo.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.NumeroProtocolo.ToLower().Contains(t) ||
                (p.Cliente != null && p.Cliente.NomeCliente.ToLower().Contains(t)) ||
                (p.NaturezaPedido != null && p.NaturezaPedido.ToLower().Contains(t)));
        }

        if (filtro.StatusAndamento.HasValue)
            query = query.Where(p => p.StatusAndamento == filtro.StatusAndamento.Value);

        if (filtro.StatusProtocolo.HasValue)
            query = query.Where(p => p.StatusProtocolo == filtro.StatusProtocolo.Value);

        var total = await query.CountAsync(ct);

        var protocolos = await query
            .OrderByDescending(p => p.DtCriacao)
            .Skip(filtro.Skip)
            .Take(filtro.Tamanho)
            .ToListAsync(ct);

        var idsProtocolos = protocolos.Select(p => p.Id).ToList();
        var escriturasPorProtocolo = await context.Escrituras.AsNoTracking()
            .Where(e => idsProtocolos.Contains(e.IdProtocolo))
            .ToDictionaryAsync(e => e.IdProtocolo, e => (e.Id, e.NumeroEscritura), ct);

        var itens = protocolos.Select(p =>
        {
            escriturasPorProtocolo.TryGetValue(p.Id, out var esc);
            return MapearProtocolo(p, esc.Id == 0 ? null : esc.Id, esc.NumeroEscritura);
        }).ToList();

        return new ResultadoPaginado<ProtocoloDto>(itens, total, filtro.Pagina, filtro.Tamanho);
    }

    public async Task<ProtocoloDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var p = await context.Protocolos.AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Ato)
            .Include(x => x.Funcionario)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (p is null) return null;

        var esc = await context.Escrituras.AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdProtocolo == p.Id, ct);

        return MapearProtocolo(p, esc?.Id, esc?.NumeroEscritura);
    }

    public async Task<ProtocoloDto?> ObterPorNumeroAsync(string numeroProtocolo, CancellationToken ct = default)
    {
        var termo = (numeroProtocolo ?? string.Empty).Trim().ToUpperInvariant();
        var p = await context.Protocolos.AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Ato)
            .Include(x => x.Funcionario)
            .FirstOrDefaultAsync(
                x => x.NumeroProtocolo.ToUpper() == termo || x.Id.ToString() == termo,
                ct);

        if (p is null) return null;

        var esc = await context.Escrituras.AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdProtocolo == p.Id, ct);

        return MapearProtocolo(p, esc?.Id, esc?.NumeroEscritura);
    }

    /// <summary>
    /// UC-12: Consultar Andamento de Processo (acesso público web/mobile sem senha a partir do nº do protocolo).
    /// Exibe as 4 etapas definidas no TCC: Aguardando documentação, Em processo de escrituração,
    /// Em processo de impressão e Finalizado – aguardando retirada.
    /// </summary>
    public async Task<AndamentoProcessoPublicoDto?> ConsultarAndamentoPublicoAsync(string numeroProtocolo, CancellationToken ct = default)
    {
        var termo = (numeroProtocolo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(termo)) return null;

        var protocolo = await context.Protocolos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Ato)
            .Include(p => p.Funcionario)
            .FirstOrDefaultAsync(
                p => p.NumeroProtocolo.ToUpper() == termo || p.Id.ToString() == termo,
                ct);

        var escritura = protocolo is not null
            ? await context.Escrituras.AsNoTracking()
                .Include(e => e.Cartorio)
                .Include(e => e.Ato)
                .Include(e => e.Funcionario)
                .FirstOrDefaultAsync(e => e.IdProtocolo == protocolo.Id, ct)
            : await context.Escrituras.AsNoTracking()
                .Include(e => e.Protocolo).ThenInclude(p => p!.Cliente)
                .Include(e => e.Cartorio)
                .Include(e => e.Ato)
                .Include(e => e.Funcionario)
                .FirstOrDefaultAsync(e => e.NumeroEscritura.ToUpper() == termo || e.Id.ToString() == termo, ct);

        if (protocolo is null && escritura is null) return null;

        protocolo ??= escritura!.Protocolo;
        var statusAtual = escritura?.StatusEscritura ?? protocolo?.StatusAndamento ?? StatusEscritura.AguardandoDocumentacao;
        var ordemAtual = (int)statusAtual;

        var etapas = new List<EtapaAndamentoDto>
        {
            new(1, StatusEscritura.AguardandoDocumentacao, "Aguardando documentação",
                "Recepção e conferência de documentos pessoais, certidões e guia de ITBI.",
                ordemAtual > 1 || statusAtual == StatusEscritura.Finalizada, statusAtual == StatusEscritura.AguardandoDocumentacao),
            new(2, StatusEscritura.EmProcessoDeEscrituracao, "Em processo de escrituração",
                "Redação da minuta da escritura pública pelo escrevente e conferência jurídica.",
                ordemAtual > 2 || statusAtual == StatusEscritura.Finalizada, statusAtual == StatusEscritura.EmProcessoDeEscrituracao),
            new(3, StatusEscritura.EmProcessoDeImpressao, "Em processo de impressão",
                "Impressão no livro de notas, coleta de assinaturas e lavratura pelo Tabelionato.",
                ordemAtual > 3 || statusAtual == StatusEscritura.Finalizada, statusAtual == StatusEscritura.EmProcessoDeImpressao),
            new(4, StatusEscritura.Finalizada, "Finalizado – aguardando retirada",
                "Escritura lavrada, índice e DOI gerados e traslado/certidão disponíveis para retirada.",
                statusAtual == StatusEscritura.Finalizada, statusAtual == StatusEscritura.Finalizada)
        };

        return new AndamentoProcessoPublicoDto(
            protocolo?.NumeroProtocolo ?? escritura!.NumeroEscritura,
            MascararNomePublico(protocolo?.Cliente?.NomeCliente ?? "Cliente Apresentante"),
            escritura?.Ato?.TipoAto ?? protocolo?.Ato?.TipoAto ?? protocolo?.NaturezaPedido ?? "Escritura Pública",
            escritura?.Cartorio?.NomeCartorio ?? "2º Tabelionato de Notas de Campinas",
            escritura?.Funcionario?.NomeFuncionario ?? protocolo?.Funcionario?.NomeFuncionario,
            statusAtual,
            EscrituraMinutaBuilder.DescreverStatusEscritura(statusAtual),
            protocolo?.DtCriacao ?? escritura!.DataInicioProcesso,
            escritura?.DtAlteracao ?? protocolo?.DtAlteracao,
            escritura?.DataLavratura ?? escritura?.DataFimProcesso,
            escritura is not null ? $"{escritura.NumeroLivro:D4}" : null,
            escritura is not null ? $"{escritura.FolhaInicial:D3} a {escritura.FolhaFinal:D3}" : null,
            statusAtual == StatusEscritura.Finalizada,
            escritura?.DocumentosRetiradosPeloCliente ?? false,
            etapas);
    }

    public async Task<IReadOnlyList<OpcaoDto>> ListarProtocolosDisponiveisParaEscrituraAsync(int? incluirProtocoloId = null, CancellationToken ct = default)
    {
        var vinculados = await context.Escrituras.AsNoTracking()
            .Where(e => incluirProtocoloId == null || e.IdProtocolo != incluirProtocoloId.Value)
            .Select(e => e.IdProtocolo)
            .ToListAsync(ct);

        return await context.Protocolos.AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.StatusProtocolo == StatusRegistro.Ativo && !vinculados.Contains(p.Id))
            .OrderByDescending(p => p.Id)
            .Select(p => new OpcaoDto(
                p.Id,
                $"Protocolo #{p.NumeroProtocolo}",
                p.Cliente != null ? p.Cliente.NomeCliente : p.NaturezaPedido))
            .ToListAsync(ct);
    }

    private static ProtocoloDto MapearProtocolo(Protocolo p, int? idEscritura, string? numeroEscritura) => new(
        p.Id,
        p.NumeroProtocolo,
        p.IdCliente,
        p.Cliente?.NomeCliente ?? string.Empty,
        p.Cliente?.CpfCliente ?? string.Empty,
        p.Cliente?.TelefoneCliente ?? string.Empty,
        p.Cliente?.EmailCliente ?? string.Empty,
        p.IdAto,
        p.Ato?.TipoAto,
        p.IdFuncionario,
        p.Funcionario?.NomeFuncionario,
        p.IdOrcamento,
        p.NaturezaPedido ?? p.Ato?.TipoAto,
        p.DocumentosApresentados,
        p.DocumentosFaltantes,
        p.ValorOrcamentoEstimado,
        p.Observacoes,
        p.StatusAndamento,
        EscrituraMinutaBuilder.DescreverStatusEscritura(p.StatusAndamento),
        p.StatusProtocolo,
        idEscritura,
        numeroEscritura,
        p.DtCriacao);

    private static string MascararNomePublico(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length <= 1) return nome;
        return $"{partes[0]} {string.Join(' ', partes.Skip(1).Select(p => p.Length > 2 ? $"{p[0]}." : p))}";
    }
}

public sealed class EscrituraQueryService(CartorioTccContext context) : IEscrituraQueryService
{
    public async Task<ResultadoPaginado<EscrituraResumoDto>> PesquisarAsync(FiltroEscrituraDto filtro, CancellationToken ct = default)
    {
        var query = AplicarFiltrosEscritura(filtro);
        var total = await query.CountAsync(ct);

        var itens = await query
            .OrderByDescending(e => e.DtCriacao)
            .Skip(filtro.Skip)
            .Take(filtro.Tamanho)
            .Select(e => new EscrituraResumoDto(
                e.Id,
                e.NumeroEscritura,
                e.IdProtocolo,
                e.Protocolo != null ? e.Protocolo.NumeroProtocolo : string.Empty,
                e.IdCartorio,
                e.Cartorio != null ? e.Cartorio.NomeCartorio : string.Empty,
                e.IdAto,
                e.Ato != null ? e.Ato.TipoAto : string.Empty,
                e.IdOutorgante,
                e.Outorgante != null ? e.Outorgante.NomeCliente : string.Empty,
                e.Outorgante != null ? e.Outorgante.CpfCliente : string.Empty,
                e.IdOutorgado,
                e.Outorgado != null ? e.Outorgado.NomeCliente : string.Empty,
                e.Outorgado != null ? e.Outorgado.CpfCliente : string.Empty,
                e.IdFuncionario,
                e.Funcionario != null ? e.Funcionario.NomeFuncionario : string.Empty,
                e.NumeroLivro,
                e.FolhaInicial,
                e.FolhaFinal,
                e.Imovel != null ? e.Imovel.NumeroRegistro : null,
                e.ValorOperacao,
                e.StatusEscritura,
                EscrituraMinutaBuilder.DescreverStatusEscritura(e.StatusEscritura),
                e.DataInicioProcesso,
                e.DataLavratura,
                e.DataFimProcesso,
                e.DataEnvioDoi,
                e.DocumentosRetiradosPeloCliente,
                e.Documentos.Count))
            .ToListAsync(ct);

        return new ResultadoPaginado<EscrituraResumoDto>(itens, total, filtro.Pagina, filtro.Tamanho);
    }

    public async Task<EscrituraDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var e = await CarregarEscrituraCompletaQuery().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : MapearEscrituraDetalhe(e);
    }

    public async Task<EscrituraDetalheDto?> ObterPorProtocoloAsync(string numeroProtocolo, CancellationToken ct = default)
    {
        var termo = (numeroProtocolo ?? string.Empty).Trim().ToUpperInvariant();
        var e = await CarregarEscrituraCompletaQuery()
            .FirstOrDefaultAsync(
                x => x.NumeroEscritura.ToUpper() == termo ||
                     (x.Protocolo != null && x.Protocolo.NumeroProtocolo.ToUpper() == termo) ||
                     x.Id.ToString() == termo ||
                     x.IdProtocolo.ToString() == termo,
                ct);

        return e is null ? null : MapearEscrituraDetalhe(e);
    }

    /// <summary>
    /// Índice automático gerado a partir das escrituras finalizadas/lavradas (seção 1.1 e 1.3 do TCC).
    /// </summary>
    public async Task<ResultadoPaginado<IndiceEscrituraDto>> ConsultarIndiceAsync(FiltroEscrituraDto filtro, CancellationToken ct = default)
    {
        var query = AplicarFiltrosEscritura(filtro)
            .Where(e => e.StatusEscritura == StatusEscritura.Finalizada);

        var total = await query.CountAsync(ct);

        var itens = await query
            .OrderByDescending(e => e.NumeroLivro)
            .ThenByDescending(e => e.FolhaInicial)
            .Skip(filtro.Skip)
            .Take(filtro.Tamanho)
            .Select(e => new IndiceEscrituraDto(
                e.Id,
                e.NumeroEscritura,
                e.Protocolo != null ? e.Protocolo.NumeroProtocolo : string.Empty,
                e.NumeroLivro,
                e.FolhaInicial,
                e.FolhaFinal,
                e.Ato != null ? e.Ato.TipoAto : string.Empty,
                e.Outorgante != null ? e.Outorgante.NomeCliente : string.Empty,
                e.Outorgante != null ? e.Outorgante.CpfCliente : string.Empty,
                e.Outorgado != null ? e.Outorgado.NomeCliente : string.Empty,
                e.Outorgado != null ? e.Outorgado.CpfCliente : string.Empty,
                e.Imovel != null ? e.Imovel.NumeroRegistro : null,
                e.ValorOperacao,
                e.DataLavratura ?? e.DataInicioProcesso,
                e.DataEnvioDoi != null,
                e.DataEnvioDoi,
                e.Documentos.Any()))
            .ToListAsync(ct);

        return new ResultadoPaginado<IndiceEscrituraDto>(itens, total, filtro.Pagina, filtro.Tamanho);
    }

    private IQueryable<Escritura> AplicarFiltrosEscritura(FiltroEscrituraDto filtro)
    {
        var query = context.Escrituras.AsNoTracking()
            .Include(e => e.Cartorio)
            .Include(e => e.Protocolo)
            .Include(e => e.Ato)
            .Include(e => e.Outorgante)
            .Include(e => e.Outorgado)
            .Include(e => e.Funcionario)
            .Include(e => e.Imovel)
            .Include(e => e.Documentos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var t = filtro.Termo.Trim().ToLowerInvariant();
            var num = Validador.SomenteNumeros(t);
            query = query.Where(e =>
                e.NumeroEscritura.ToLower().Contains(t) ||
                (e.Protocolo != null && e.Protocolo.NumeroProtocolo.ToLower().Contains(t)) ||
                (e.Outorgante != null && (e.Outorgante.NomeCliente.ToLower().Contains(t) || (!string.IsNullOrEmpty(num) && e.Outorgante.CpfCliente.Contains(num)))) ||
                (e.Outorgado != null && (e.Outorgado.NomeCliente.ToLower().Contains(t) || (!string.IsNullOrEmpty(num) && e.Outorgado.CpfCliente.Contains(num)))) ||
                (e.Imovel != null && e.Imovel.NumeroRegistro.ToLower().Contains(t)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.NumeroProtocolo))
        {
            var np = filtro.NumeroProtocolo.Trim().ToLowerInvariant();
            query = query.Where(e => e.Protocolo != null && e.Protocolo.NumeroProtocolo.ToLower().Contains(np));
        }

        if (filtro.Status.HasValue)
            query = query.Where(e => e.StatusEscritura == filtro.Status.Value);

        if (filtro.IdAto is > 0)
            query = query.Where(e => e.IdAto == filtro.IdAto.Value);

        if (filtro.NumeroLivro is > 0)
            query = query.Where(e => e.NumeroLivro == filtro.NumeroLivro.Value);

        if (filtro.DataInicio.HasValue)
        {
            var dtIni = filtro.DataInicio.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(e => e.DataInicioProcesso >= dtIni);
        }

        if (filtro.DataFim.HasValue)
        {
            var dtFim = filtro.DataFim.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(e => e.DataInicioProcesso <= dtFim);
        }

        return query;
    }

    private IQueryable<Escritura> CarregarEscrituraCompletaQuery() =>
        context.Escrituras.AsNoTracking()
            .Include(x => x.Cartorio)
            .Include(x => x.Protocolo)
            .Include(x => x.Ato)
            .Include(x => x.Funcionario)
            .Include(x => x.Outorgante).ThenInclude(c => c!.Conjuge)
            .Include(x => x.Outorgante).ThenInclude(c => c!.CertidaoCasamento)
            .Include(x => x.Outorgado).ThenInclude(c => c!.Conjuge)
            .Include(x => x.Outorgado).ThenInclude(c => c!.CertidaoCasamento)
            .Include(x => x.Imovel)
            .Include(x => x.CertidaoOutroOrgao)
            .Include(x => x.Documentos);

    private static EscrituraDetalheDto MapearEscrituraDetalhe(Escritura e)
    {
        var textoMinuta = !string.IsNullOrWhiteSpace(e.TextoMinuta)
            ? e.TextoMinuta!
            : (e.Cartorio is not null && e.Ato is not null && e.Protocolo is not null && e.Funcionario is not null &&
               e.Outorgante is not null && e.Outorgado is not null
                ? EscrituraMinutaBuilder.GerarTextoMinuta(
                    e.Cartorio,
                    e.Ato,
                    e.Protocolo,
                    e.Funcionario,
                    e.Outorgante,
                    e.Outorgado,
                    e.Imovel,
                    e.CertidaoOutroOrgao,
                    e.NumeroLivro,
                    e.FolhaInicial,
                    e.FolhaFinal,
                    e.ValorOperacao,
                    e.DataLavratura ?? e.DataInicioProcesso)
                : string.Empty);

        return new EscrituraDetalheDto(
            e.Id,
            e.NumeroEscritura,
            e.IdCartorio,
            e.Cartorio?.NomeCartorio ?? string.Empty,
            e.IdProtocolo,
            e.Protocolo?.NumeroProtocolo ?? string.Empty,
            e.IdAto,
            e.Ato?.TipoAto ?? string.Empty,
            e.IdFuncionario,
            e.Funcionario?.NomeFuncionario ?? string.Empty,
            ClienteQueryService.MapearClienteDetalhe(e.Outorgante!),
            ClienteQueryService.MapearClienteDetalhe(e.Outorgado!),
            e.Imovel is null
                ? null
                : new ImovelDto(
                    e.Imovel.Id,
                    e.Imovel.IdCliente,
                    e.Imovel.NumeroRegistro,
                    e.Imovel.ValorVenal,
                    e.Imovel.NomePrefeitura,
                    e.Imovel.DescricaoImovel,
                    e.Imovel.ValorImovel,
                    e.Imovel.AreaMetrosQuadrados,
                    e.Imovel.ValorPorMetroQuadrado,
                    e.Imovel.InscricaoMunicipal,
                    e.Imovel.CartorioRegistroImoveis,
                    EnderecoDto.DeDomain(e.Imovel.Endereco)),
            e.CertidaoOutroOrgao is null
                ? null
                : new CertidaoOutroOrgaoDto(
                    e.CertidaoOutroOrgao.Id,
                    e.CertidaoOutroOrgao.CartorioCertidaoOutroOrgao,
                    e.CertidaoOutroOrgao.LivroCertidaoOutroOrgao,
                    e.CertidaoOutroOrgao.FolhasCertidaoOutroOrgao,
                    e.CertidaoOutroOrgao.TermoCertidaoOutroOrgao,
                    e.CertidaoOutroOrgao.DataCertidaoOutroOrgao),
            e.NumeroLivro,
            e.FolhaInicial,
            e.FolhaFinal,
            e.ValorOperacao,
            textoMinuta,
            e.StatusEscritura,
            EscrituraMinutaBuilder.DescreverStatusEscritura(e.StatusEscritura),
            e.MotivoInativacao,
            e.DataInicioProcesso,
            e.DataLavratura,
            e.DataFimProcesso,
            e.DataEnvioDoi,
            e.DocumentosRetiradosPeloCliente,
            e.DataRetiradaDocumentos,
            e.ObservacoesRetirada,
            e.Documentos
                .OrderByDescending(d => d.DtCriacao)
                .Select(d => new DocumentoDigitalizadoDto(
                    d.Id,
                    d.IdEscritura,
                    d.IdCliente,
                    e.Outorgante?.NomeCliente,
                    e.Outorgante?.CpfCliente,
                    d.TipoDocumento,
                    d.TipoDocumento.ToString(),
                    d.NomeArquivo,
                    d.ContentType,
                    d.TamanhoBytes,
                    d.HashSha256,
                    d.Observacao,
                    d.DtCriacao))
                .ToList());
    }
}

public sealed class RelatorioQueryService(CartorioTccContext context) : IRelatorioQueryService
{
    public async Task<RelatorioResultadoDto> GerarRelatorioAsync(
        FiltroRelatorioRequest filtro,
        string usuarioSolicitante,
        CancellationToken ct = default)
    {
        // Regra de Negócio do UC-04: "O sistema deve armazenar os logs dos usuários que geraram os relatórios."
        context.HistoricoAuditorias.Add(new HistoricoAuditoria(
            "Tipos_Relatorios",
            ((int)filtro.TipoRelatorio).ToString(),
            AcaoAuditoria.Inclusao,
            usuarioSolicitante,
            DateTime.Now,
            null,
            $"Relatório gerado: {filtro.TipoRelatorio}"));
        await context.SaveChangesAsync(ct);

        var itens = new List<RelatorioLinhaDto>();
        string titulo;

        switch (filtro.TipoRelatorio)
        {
            case TipoRelatorioEnum.EscriturasCriadas:
                titulo = "Relatório de Escrituras Criadas e Lavradas";
                var escrituras = await context.Escrituras.AsNoTracking()
                    .Include(e => e.Protocolo)
                    .Include(e => e.Ato)
                    .Include(e => e.Outorgante)
                    .Include(e => e.Outorgado)
                    .Include(e => e.Imovel)
                    .OrderByDescending(e => e.DataInicioProcesso)
                    .ToListAsync(ct);

                itens.AddRange(escrituras.Select(e => new RelatorioLinhaDto(
                    e.Id,
                    e.NumeroEscritura,
                    $"{e.Ato?.TipoAto} — Protocolo #{e.Protocolo?.NumeroProtocolo}",
                    $"Outorgante: {e.Outorgante?.NomeCliente} ({e.Outorgante?.CpfCliente})",
                    $"Outorgado: {e.Outorgado?.NomeCliente} ({e.Outorgado?.CpfCliente})",
                    $"Livro {e.NumeroLivro:D4} / Fls. {e.FolhaInicial:D3}-{e.FolhaFinal:D3}",
                    EscrituraMinutaBuilder.DescreverStatusEscritura(e.StatusEscritura),
                    e.ValorOperacao,
                    e.DataLavratura ?? e.DataInicioProcesso,
                    $"Matrícula: {e.Imovel?.NumeroRegistro ?? "N/A"} | Prefeitura: {e.Imovel?.NomePrefeitura ?? "N/A"}")));
                break;

            case TipoRelatorioEnum.IndiceLivroNotas:
                titulo = "Relatório do Índice Geral do Livro de Notas e Controle DOI";
                var lavradas = await context.Escrituras.AsNoTracking()
                    .Include(e => e.Protocolo)
                    .Include(e => e.Ato)
                    .Include(e => e.Outorgante)
                    .Include(e => e.Outorgado)
                    .Include(e => e.Imovel)
                    .Where(e => e.StatusEscritura == StatusEscritura.Finalizada)
                    .OrderBy(e => e.NumeroLivro).ThenBy(e => e.FolhaInicial)
                    .ToListAsync(ct);

                itens.AddRange(lavradas.Select(e => new RelatorioLinhaDto(
                    e.Id,
                    $"L.{e.NumeroLivro:D4}/F.{e.FolhaInicial:D3}",
                    $"{e.NumeroEscritura} — {e.Ato?.TipoAto}",
                    e.Outorgante?.NomeCliente ?? string.Empty,
                    e.Outorgado?.NomeCliente ?? string.Empty,
                    e.Protocolo?.NumeroProtocolo ?? string.Empty,
                    e.DataEnvioDoi != null ? $"DOI enviada em {e.DataEnvioDoi:dd/MM/yyyy}" : "DOI pendente",
                    e.ValorOperacao,
                    e.DataLavratura ?? e.DataInicioProcesso,
                    $"Matrícula Imóvel: {e.Imovel?.NumeroRegistro ?? "Sem imóvel"}")));
                break;

            case TipoRelatorioEnum.ProtocolosAtendimento:
                titulo = "Relatório de Protocolos de Atendimento";
                var protocolos = await context.Protocolos.AsNoTracking()
                    .Include(p => p.Cliente)
                    .Include(p => p.Ato)
                    .Include(p => p.Funcionario)
                    .OrderByDescending(p => p.DtCriacao)
                    .ToListAsync(ct);

                itens.AddRange(protocolos.Select(p => new RelatorioLinhaDto(
                    p.Id,
                    p.NumeroProtocolo,
                    p.NaturezaPedido ?? p.Ato?.TipoAto ?? "Atendimento Notarial",
                    p.Cliente?.NomeCliente ?? string.Empty,
                    $"Escrevente: {p.Funcionario?.NomeFuncionario ?? "Não atribuído"}",
                    p.Ato?.TipoAto ?? "Geral",
                    EscrituraMinutaBuilder.DescreverStatusEscritura(p.StatusAndamento),
                    p.ValorOrcamentoEstimado,
                    p.DtCriacao,
                    $"Apresentados: {p.DocumentosApresentados ?? "-"} | Faltantes: {p.DocumentosFaltantes ?? "Nenhum"}")));
                break;

            case TipoRelatorioEnum.ClientesCadastrados:
                titulo = "Relatório de Clientes e Partes Interessadas Cadastradas";
                var clientes = await context.Clientes.AsNoTracking()
                    .Include(c => c.Conjuge)
                    .OrderBy(c => c.NomeCliente)
                    .ToListAsync(ct);

                itens.AddRange(clientes.Select(c => new RelatorioLinhaDto(
                    c.Id,
                    c.CpfCliente,
                    c.NomeCliente,
                    $"RG: {c.RgCliente} ({c.OrgaoExpedidorCliente}/{c.EstadoOrgaoExpedidorCliente})",
                    c.Conjuge != null ? $"Cônjuge: {c.Conjuge.NomeConjuge}" : $"Estado Civil: {c.EstadoCivilCliente}",
                    c.ProfissaoCliente,
                    c.StatusCliente == StatusRegistro.Ativo ? "Ativo" : "Inativo",
                    null,
                    c.DtCriacao,
                    $"{c.Endereco.Formatado} | Tel.: {c.TelefoneCliente} | E-mail: {c.EmailCliente}")));
                break;

            case TipoRelatorioEnum.OrcamentosRealizados:
                titulo = "Relatório de Orçamentos e Emolumentos Calculados (Tabela CNB/SP)";
                var orcamentos = await context.Orcamentos.AsNoTracking()
                    .Include(o => o.Cliente)
                    .OrderByDescending(o => o.DtCriacao)
                    .ToListAsync(ct);

                itens.AddRange(orcamentos.Select(o => new RelatorioLinhaDto(
                    o.Id,
                    $"ORC-{o.Id:D4}",
                    o.FaixaDescricao,
                    o.Cliente?.NomeCliente ?? "Simulação de Balcão",
                    $"Base do Ato: {o.ValorBaseAto:C2}",
                    o.Oneroso ? "Ato Oneroso" : "Ato Não Oneroso",
                    "Calculado",
                    o.ValorTotalOrcamento,
                    o.DtCriacao,
                    $"Tabelião: {o.EmolumentosTabelionato:C2} | Estado/IPESP/TJ/MP: {(o.EmolumentosEstado + o.ContribuicaoIpesp + o.TaxaTribunalJustica + o.FundoMinisterioPublico):C2} | RI: {o.PrevisaoRegistroImoveis:C2}")));
                break;

            default:
                titulo = "Relatório de Auditoria e Logs do Sistema";
                var logs = await context.HistoricoAuditorias.AsNoTracking()
                    .OrderByDescending(h => h.DataOcorrencia)
                    .Take(100)
                    .ToListAsync(ct);

                itens.AddRange(logs.Select(h => new RelatorioLinhaDto(
                    h.Id,
                    $"AUD-{h.Id:D5}",
                    $"Tabela: {h.Tabela} (ID #{h.ChavePrimaria})",
                    $"Usuário: {h.LoginUsuario}",
                    $"Ação: {h.Acao}",
                    h.Tabela,
                    h.Acao.ToString(),
                    null,
                    h.DataOcorrencia,
                    h.ValoresNovosJson ?? h.ValoresAntigosJson ?? "-")));
                break;
        }

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var t = filtro.Termo.Trim().ToLowerInvariant();
            itens = itens.Where(i =>
                i.Codigo.ToLowerInvariant().Contains(t) ||
                i.Titulo.ToLowerInvariant().Contains(t) ||
                i.PartePrincipal.ToLowerInvariant().Contains(t) ||
                i.ParteSecundaria.ToLowerInvariant().Contains(t)).ToList();
        }

        return new RelatorioResultadoDto(
            filtro.TipoRelatorio,
            titulo,
            DateTime.Now,
            usuarioSolicitante,
            itens.Count,
            itens.Sum(x => x.Valor ?? 0m),
            itens);
    }

    public async Task<DashboardResumoDto> ObterDashboardResumoAsync(CancellationToken ct = default)
    {
        var totalClientes = await context.Clientes.AsNoTracking()
            .CountAsync(c => c.StatusCliente == StatusRegistro.Ativo, ct);

        var totalProtocolos = await context.Protocolos.AsNoTracking()
            .CountAsync(p => p.StatusProtocolo == StatusRegistro.Ativo, ct);

        var escrituras = await context.Escrituras.AsNoTracking()
            .Include(e => e.Ato)
            .Include(e => e.Cartorio)
            .Include(e => e.Protocolo)
            .Include(e => e.Outorgante)
            .Include(e => e.Outorgado)
            .Include(e => e.Funcionario)
            .Include(e => e.Imovel)
            .Include(e => e.Documentos)
            .OrderByDescending(e => e.DtCriacao)
            .ToListAsync(ct);

        var emAndamento = escrituras.Count(e =>
            e.StatusEscritura is StatusEscritura.AguardandoDocumentacao
                or StatusEscritura.EmProcessoDeEscrituracao
                or StatusEscritura.EmProcessoDeImpressao);

        var finalizadas = escrituras.Count(e => e.StatusEscritura == StatusEscritura.Finalizada);
        var pendentesDoi = escrituras.Count(e => e.StatusEscritura == StatusEscritura.Finalizada && e.DataEnvioDoi == null);
        var valorTotal = escrituras.Where(e => e.StatusEscritura != StatusEscritura.Inativa).Sum(e => e.ValorOperacao);

        var totalDocs = await context.DocumentosDigitalizados.AsNoTracking().CountAsync(ct);
        var ultimoBackup = await context.Backups.AsNoTracking()
            .OrderByDescending(b => b.DataBackup)
            .Select(b => (DateTime?)b.DataBackup)
            .FirstOrDefaultAsync(ct);

        var porStatus = escrituras
            .GroupBy(e => EscrituraMinutaBuilder.DescreverStatusEscritura(e.StatusEscritura))
            .ToDictionary(g => g.Key, g => g.Count());

        var porAto = escrituras
            .GroupBy(e => e.Ato?.TipoAto ?? "Outros")
            .ToDictionary(g => g.Key, g => g.Count());

        var ultimas = escrituras.Take(6).Select(e => new EscrituraResumoDto(
            e.Id,
            e.NumeroEscritura,
            e.IdProtocolo,
            e.Protocolo?.NumeroProtocolo ?? string.Empty,
            e.IdCartorio,
            e.Cartorio?.NomeCartorio ?? string.Empty,
            e.IdAto,
            e.Ato?.TipoAto ?? string.Empty,
            e.IdOutorgante,
            e.Outorgante?.NomeCliente ?? string.Empty,
            e.Outorgante?.CpfCliente ?? string.Empty,
            e.IdOutorgado,
            e.Outorgado?.NomeCliente ?? string.Empty,
            e.Outorgado?.CpfCliente ?? string.Empty,
            e.IdFuncionario,
            e.Funcionario?.NomeFuncionario ?? string.Empty,
            e.NumeroLivro,
            e.FolhaInicial,
            e.FolhaFinal,
            e.Imovel?.NumeroRegistro,
            e.ValorOperacao,
            e.StatusEscritura,
            EscrituraMinutaBuilder.DescreverStatusEscritura(e.StatusEscritura),
            e.DataInicioProcesso,
            e.DataLavratura,
            e.DataFimProcesso,
            e.DataEnvioDoi,
            e.DocumentosRetiradosPeloCliente,
            e.Documentos.Count)).ToList();

        return new DashboardResumoDto(
            totalClientes,
            totalProtocolos,
            emAndamento,
            finalizadas,
            pendentesDoi,
            totalDocs,
            valorTotal,
            ultimoBackup,
            porStatus,
            porAto,
            ultimas);
    }

    public async Task<IReadOnlyList<BackupDto>> ListarBackupsAsync(CancellationToken ct = default) =>
        await context.Backups.AsNoTracking()
            .Include(b => b.Funcionario)
            .OrderByDescending(b => b.DataBackup)
            .Select(b => new BackupDto(
                b.Id,
                b.DataBackup,
                b.DataAgendada,
                b.DescricaoBackup,
                b.IdFuncionario,
                b.Funcionario != null ? b.Funcionario.NomeFuncionario : "Administrador SAEC",
                b.Manual,
                b.Criptografado,
                b.Executado,
                b.NomeArquivoGerado,
                b.TamanhoBytes,
                b.HashIntegridade))
            .ToListAsync(ct);
}
