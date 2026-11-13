using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Enums;

namespace CartorioTcc.Business.Dtos;

// --- UC-06: Orçamentos / Tabela de Custas CNB-SP ---
public sealed record CalcularOrcamentoRequest(
    decimal ValorBaseAto,
    int? IdCliente = null,
    bool Oneroso = true,
    bool Hipoteca = false,
    bool Ufesp2500 = false,
    bool Salarios30 = false,
    bool CertidaoInclusa = true,
    decimal OutrosValores = 0m);

public sealed record OrcamentoDto(
    int Id,
    int? IdCliente,
    string? NomeCliente,
    decimal ValorBaseAto,
    bool Oneroso,
    bool Hipoteca,
    bool Ufesp2500,
    bool Salarios30,
    bool CertidaoInclusa,
    decimal OutrosValores,
    string FaixaDescricao,
    decimal EmolumentosTabelionato,
    decimal EmolumentosEstado,
    decimal ContribuicaoIpesp,
    decimal FundoRegistroCivil,
    decimal TaxaTribunalJustica,
    decimal FundoMinisterioPublico,
    decimal IssqnEstimado,
    decimal ValorCertidao,
    decimal PrevisaoRegistroImoveis,
    decimal ValorTotalOrcamento,
    DateTime DtCriacao);

public sealed record FaixaCustasDto(
    int Ordem,
    string DescricaoFaixa,
    decimal ValorMinimo,
    decimal ValorMaximo,
    decimal EmolumentosTabeliao,
    decimal ReceitaEstado,
    decimal ContribuicaoIpesp,
    decimal FundoRegistroCivil,
    decimal TaxaTribunalJustica,
    decimal FundoMinisterioPublico,
    decimal TotalNotarial,
    decimal EstimativaRegistroImoveis);

// --- UC-07 e UC-12: Protocolos e Consulta Pública de Andamento ---
public sealed record ProtocoloDto(
    int Id,
    string NumeroProtocolo,
    int IdCliente,
    string NomeCliente,
    string CpfCliente,
    string TelefoneCliente,
    string EmailCliente,
    int? IdAto,
    string? TipoAto,
    int? IdFuncionario,
    string? NomeEscrevente,
    int? IdOrcamento,
    string? NaturezaPedido,
    string? DocumentosApresentados,
    string? DocumentosFaltantes,
    decimal? ValorOrcamentoEstimado,
    string? Observacoes,
    StatusEscritura StatusAndamento,
    string StatusAndamentoDescricao,
    StatusRegistro StatusProtocolo,
    int? IdEscrituraVinculada,
    string? NumeroEscrituraVinculada,
    DateTime DtCriacao);

public sealed record SalvarProtocoloCommand(
    int IdCliente,
    int? IdAto = null,
    int? IdFuncionario = null,
    int? IdOrcamento = null,
    string? NaturezaPedido = null,
    string? DocumentosApresentados = null,
    string? DocumentosFaltantes = null,
    decimal? ValorOrcamentoEstimado = null,
    string? Observacoes = null,
    StatusEscritura StatusAndamento = StatusEscritura.AguardandoDocumentacao);

public sealed class FiltroProtocoloDto : FiltroPaginado
{
    public StatusEscritura? StatusAndamento { get; set; }
    public StatusRegistro? StatusProtocolo { get; set; }
}

public sealed record EtapaAndamentoDto(
    int Ordem,
    StatusEscritura Etapa,
    string Titulo,
    string Descricao,
    bool Concluida,
    bool Atual);

public sealed record AndamentoProcessoPublicoDto(
    string NumeroProtocolo,
    string NomeApresentante,
    string NaturezaAto,
    string CartorioNome,
    string? EscreventeResponsavel,
    StatusEscritura StatusAtual,
    string StatusAtualDescricao,
    DateTime DataSolicitacao,
    DateTime? DataUltimaAtualizacao,
    DateTime? DataConclusao,
    string? NumeroLivro,
    string? Folhas,
    bool DocumentosProntosParaRetirada,
    bool DocumentosRetirados,
    IReadOnlyList<EtapaAndamentoDto> Etapas);

// --- UC-09, UC-10, UC-11: Escrituras, Imóveis, Certidões e DOI ---
public sealed record ImovelDto(
    int? Id,
    int? IdCliente,
    string NumeroRegistro,
    decimal ValorVenal,
    string NomePrefeitura,
    string? DescricaoImovel,
    decimal ValorImovel,
    decimal? AreaMetrosQuadrados,
    decimal? ValorPorMetroQuadrado,
    string? InscricaoMunicipal,
    string? CartorioRegistroImoveis,
    EnderecoDto Endereco);

public sealed record CertidaoOutroOrgaoDto(
    int? Id,
    string CartorioCertidaoOutroOrgao,
    string LivroCertidaoOutroOrgao,
    string FolhasCertidaoOutroOrgao,
    string TermoCertidaoOutroOrgao,
    DateOnly DataCertidaoOutroOrgao);

public sealed record DocumentoDigitalizadoDto(
    int Id,
    int? IdEscritura,
    int? IdCliente,
    string? NomeCliente,
    string? CpfCliente,
    TipoDocumentoDigitalizado TipoDocumento,
    string TipoDocumentoDescricao,
    string NomeArquivo,
    string ContentType,
    long TamanhoBytes,
    string HashSha256,
    string? Observacao,
    DateTime DtCriacao);

public sealed record AnexarDocumentoCommand(
    int? IdEscritura,
    int? IdCliente,
    TipoDocumentoDigitalizado TipoDocumento,
    string NomeArquivo,
    string ContentType,
    byte[] ConteudoPdf,
    string? Observacao = null);

public sealed record EscrituraResumoDto(
    int Id,
    string NumeroEscritura,
    int IdProtocolo,
    string NumeroProtocolo,
    int IdCartorio,
    string NomeCartorio,
    int IdAto,
    string TipoAto,
    int IdOutorgante,
    string NomeOutorgante,
    string CpfOutorgante,
    int IdOutorgado,
    string NomeOutorgado,
    string CpfOutorgado,
    int IdFuncionario,
    string NomeEscrevente,
    int NumeroLivro,
    int FolhaInicial,
    int FolhaFinal,
    string? MatriculaImovel,
    decimal ValorOperacao,
    StatusEscritura StatusEscritura,
    string StatusEscrituraDescricao,
    DateTime DataInicioProcesso,
    DateTime? DataLavratura,
    DateTime? DataFimProcesso,
    DateTime? DataEnvioDoi,
    bool DocumentosRetiradosPeloCliente,
    int QuantidadeDocumentosPdf);

public sealed record EscrituraDetalheDto(
    int Id,
    string NumeroEscritura,
    int IdCartorio,
    string NomeCartorio,
    int IdProtocolo,
    string NumeroProtocolo,
    int IdAto,
    string TipoAto,
    int IdFuncionario,
    string NomeEscrevente,
    ClienteDetalheDto Outorgante,
    ClienteDetalheDto Outorgado,
    ImovelDto? Imovel,
    CertidaoOutroOrgaoDto? CertidaoOutroOrgao,
    int NumeroLivro,
    int FolhaInicial,
    int FolhaFinal,
    decimal ValorOperacao,
    string TextoMinuta,
    StatusEscritura StatusEscritura,
    string StatusEscrituraDescricao,
    string? MotivoInativacao,
    DateTime DataInicioProcesso,
    DateTime? DataLavratura,
    DateTime? DataFimProcesso,
    DateTime? DataEnvioDoi,
    bool DocumentosRetiradosPeloCliente,
    DateTime? DataRetiradaDocumentos,
    string? ObservacoesRetirada,
    IReadOnlyList<DocumentoDigitalizadoDto> Documentos);

public sealed record SalvarEscrituraCommand(
    int IdCartorio,
    int IdProtocolo,
    int IdAto,
    int IdFuncionario,
    int? IdOutorgante,
    SalvarClienteCommand? NovoOutorgante,
    int? IdOutorgado,
    SalvarClienteCommand? NovoOutorgado,
    ImovelDto? Imovel,
    CertidaoOutroOrgaoDto? CertidaoOutroOrgao,
    int NumeroLivro,
    int FolhaInicial,
    int FolhaFinal,
    decimal ValorOperacao,
    string? TextoMinuta,
    StatusEscritura StatusEscritura = StatusEscritura.EmProcessoDeEscrituracao);

public sealed record FinalizarEscrituraCommand(int IdEscritura, string? TextoMinutaDefinitivo = null);

public sealed record InativarEscrituraCommand(int IdEscritura, string Motivo);

public sealed record RegistrarRetiradaProtocoloCommand(int IdEscritura, string? Observacoes = null);

public sealed class FiltroEscrituraDto : FiltroPaginado
{
    public StatusEscritura? Status { get; set; }
    public int? IdAto { get; set; }
    public string? NumeroProtocolo { get; set; }
    public int? NumeroLivro { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
}

// --- Índice de Livros e Notas (item 1.3 e 1.1 do TCC) ---
public sealed record IndiceEscrituraDto(
    int IdEscritura,
    string NumeroEscritura,
    string NumeroProtocolo,
    int NumeroLivro,
    int FolhaInicial,
    int FolhaFinal,
    string TipoAto,
    string NomeOutorgante,
    string CpfOutorgante,
    string NomeOutorgado,
    string CpfOutorgado,
    string? MatriculaImovel,
    decimal ValorOperacao,
    DateTime DataLavratura,
    bool DoiGerada,
    DateTime? DataEnvioDoi,
    bool PossuiPdfDigitalizado);

// --- UC-03 e UC-05: Backups Criptografados ---
public sealed record BackupDto(
    int Id,
    DateTime DataBackup,
    DateTime? DataAgendada,
    string DescricaoBackup,
    int? IdFuncionario,
    string NomeFuncionario,
    bool Manual,
    bool Criptografado,
    bool Executado,
    string NomeArquivoGerado,
    long TamanhoBytes,
    string HashIntegridade);

public sealed record AgendarBackupCommand(
    DateTime DataAgendada,
    string? DescricaoBackup = null,
    int? IdFuncionario = null);

public sealed record RealizarBackupManualCommand(
    string? DescricaoBackup = null,
    int? IdFuncionario = null);

// --- UC-04: Relatórios e Dashboard ---
public sealed record FiltroRelatorioRequest(
    TipoRelatorioEnum TipoRelatorio,
    DateOnly? DataInicio = null,
    DateOnly? DataFim = null,
    string? Termo = null,
    StatusEscritura? StatusEscritura = null);

public sealed record RelatorioLinhaDto(
    int IdReferencia,
    string Codigo,
    string Titulo,
    string PartePrincipal,
    string ParteSecundaria,
    string Categoria,
    string Status,
    decimal? Valor,
    DateTime DataReferencia,
    string Detalhes);

public sealed record RelatorioResultadoDto(
    TipoRelatorioEnum TipoRelatorio,
    string TituloRelatorio,
    DateTime DataGeracao,
    string GeradoPor,
    int TotalRegistros,
    decimal SomaValores,
    IReadOnlyList<RelatorioLinhaDto> Itens);

public sealed record DashboardResumoDto(
    int TotalClientesAtivos,
    int TotalProtocolosAtivos,
    int TotalEscriturasEmAndamento,
    int TotalEscriturasFinalizadas,
    int EscriturasPendentesDoi,
    int TotalDocumentosDigitalizados,
    decimal ValorTotalEscriturado,
    DateTime? UltimoBackupRealizado,
    IReadOnlyDictionary<string, int> EscriturasPorStatus,
    IReadOnlyDictionary<string, int> EscriturasPorNaturezaAto,
    IReadOnlyList<EscrituraResumoDto> UltimasEscrituras);
