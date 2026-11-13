using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Entities;

namespace CartorioTcc.Business.Interfaces;

// --- Repositórios (Escrita / Commands) ---
public interface IPermissaoRepository : IRepository<PaginaPerfil>
{
    Task<bool> ExistePermissaoAsync(int idPerfil, int idPagina, int? ignorarId = null, CancellationToken ct = default);
}

public interface IFuncionarioRepository : IRepository<Funcionario>
{
    Task<bool> ExisteCpfOuEmailAsync(string cpf, string email, int? ignorarId = null, CancellationToken ct = default);
}

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> ObterCompletoPorIdAsync(int id, CancellationToken ct = default);
    Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken ct = default);
    Task<bool> ExisteCpfAsync(string cpf, int? ignorarId = null, CancellationToken ct = default);
}

public interface IOrcamentoRepository : IRepository<Orcamento>;

public interface IProtocoloRepository : IRepository<Protocolo>
{
    Task<Protocolo?> ObterPorNumeroAsync(string numeroProtocolo, CancellationToken ct = default);
    Task<int> ObterProximoSequencialAsync(int ano, CancellationToken ct = default);
}

public interface IEscrituraRepository : IRepository<Escritura>
{
    Task<Escritura?> ObterCompletaPorIdAsync(int id, CancellationToken ct = default);
    Task<Escritura?> ObterCompletaPorNumeroOuProtocoloAsync(string numeroOuProtocolo, CancellationToken ct = default);
    Task<bool> ExisteParaProtocoloAsync(int idProtocolo, int? ignorarId = null, CancellationToken ct = default);
    Task<int> ObterProximoSequencialEscrituraAsync(CancellationToken ct = default);
    void AdicionarImovel(Imovel imovel);
    void AdicionarCertidaoOutroOrgao(CertidaoOutroOrgao certidao);
    void AdicionarDocumento(DocumentoDigitalizado documento);
    Task<DocumentoDigitalizado?> ObterDocumentoPorIdAsync(int idDocumento, CancellationToken ct = default);
    void RemoverDocumento(DocumentoDigitalizado documento);
}

public interface IBackupRepository : IRepository<BackupRegistro>
{
    Task<IReadOnlyList<BackupRegistro>> ObterAgendadosPendentesAsync(DateTime ateData, CancellationToken ct = default);
}

// --- Query Services (Leitura Otimizada / AsNoTracking / CQRS) ---
public interface ICatalogoQueryService
{
    Task<IReadOnlyList<PerfilDto>> ListarPerfisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PaginaDto>> ListarPaginasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PermissaoDto>> ListarPermissoesAsync(int? idPerfil = null, CancellationToken ct = default);
    Task<PermissaoDto?> ObterPermissaoPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CartorioDto>> ListarCartoriosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AtoDto>> ListarAtosAsync(CancellationToken ct = default);
}

public interface IFuncionarioQueryService
{
    Task<ResultadoPaginado<FuncionarioResumoDto>> PesquisarAsync(FiltroFuncionarioDto filtro, CancellationToken ct = default);
    Task<FuncionarioDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<OpcaoDto>> ListarOpcoesAtivasAsync(CancellationToken ct = default);
}

public interface IClienteQueryService
{
    Task<ResultadoPaginado<ClienteResumoDto>> PesquisarAsync(FiltroClienteDto filtro, CancellationToken ct = default);
    Task<ClienteDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<ClienteDetalheDto?> ObterPorCpfAsync(string cpf, CancellationToken ct = default);
    Task<IReadOnlyList<OpcaoDto>> ListarOpcoesAtivasAsync(string? termo = null, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentoDigitalizadoDto>> ConsultarDocumentosPorClienteAsync(string termoNomeOuCpf, CancellationToken ct = default);
}

public interface IOrcamentoQueryService
{
    Task<IReadOnlyList<OrcamentoDto>> ListarRecentesAsync(int limite = 50, CancellationToken ct = default);
    Task<OrcamentoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    IReadOnlyList<FaixaCustasDto> ListarFaixasTabelaCustas();
}

public interface IProtocoloQueryService
{
    Task<ResultadoPaginado<ProtocoloDto>> PesquisarAsync(FiltroProtocoloDto filtro, CancellationToken ct = default);
    Task<ProtocoloDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<ProtocoloDto?> ObterPorNumeroAsync(string numeroProtocolo, CancellationToken ct = default);
    Task<AndamentoProcessoPublicoDto?> ConsultarAndamentoPublicoAsync(string numeroProtocolo, CancellationToken ct = default);
    Task<IReadOnlyList<OpcaoDto>> ListarProtocolosDisponiveisParaEscrituraAsync(int? incluirProtocoloId = null, CancellationToken ct = default);
}

public interface IEscrituraQueryService
{
    Task<ResultadoPaginado<EscrituraResumoDto>> PesquisarAsync(FiltroEscrituraDto filtro, CancellationToken ct = default);
    Task<EscrituraDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<EscrituraDetalheDto?> ObterPorProtocoloAsync(string numeroProtocolo, CancellationToken ct = default);
    Task<ResultadoPaginado<IndiceEscrituraDto>> ConsultarIndiceAsync(FiltroEscrituraDto filtro, CancellationToken ct = default);
}

public interface IRelatorioQueryService
{
    Task<RelatorioResultadoDto> GerarRelatorioAsync(FiltroRelatorioRequest filtro, string usuarioSolicitante, CancellationToken ct = default);
    Task<DashboardResumoDto> ObterDashboardResumoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BackupDto>> ListarBackupsAsync(CancellationToken ct = default);
}

// --- Serviços de Infraestrutura / Integração ---
public interface ICepExternalService
{
    Task<CepConsultaDto?> ConsultarCepAsync(string cep, CancellationToken ct = default);
}

public interface IDoiExportGenerator
{
    ArquivoGerado GerarXmlDoi(EscrituraDetalheDto escritura, DateTime dataGeracao);
    ArquivoGerado GerarXmlDoiLote(IReadOnlyList<EscrituraDetalheDto> escrituras, DateTime dataGeracao);
}

public interface IPdfGeneratorService
{
    ArquivoGerado GerarCertidaoEscrituraPdf(EscrituraDetalheDto escritura, DateTime dataEmissao, string emitidoPor);
    ArquivoGerado GerarProtocoloAtendimentoPdf(ProtocoloDto protocolo);
    ArquivoGerado GerarProtocoloRetiradaPdf(EscrituraDetalheDto escritura);
    ArquivoGerado GerarOrcamentoPdf(OrcamentoDto orcamento);
    ArquivoGerado GerarRelatorioPdf(RelatorioResultadoDto relatorio);
}

public interface IBackupCriptografadoProvider
{
    Task<(string NomeArquivo, byte[] ConteudoCriptografado, string HashSha256)> GerarPacoteBackupCriptografadoAsync(
        string descricao,
        CancellationToken ct = default);
}

// --- Serviços de Negócio (Application / Domain Services) ---
public interface IPermissaoService
{
    Task<PermissaoDto?> IncluirAsync(SalvarPermissaoCommand command, CancellationToken ct = default);
    Task<PermissaoDto?> AlterarAsync(int id, SalvarPermissaoCommand command, CancellationToken ct = default);
    Task<bool> ExcluirAsync(int id, CancellationToken ct = default);
}

public interface IFuncionarioService
{
    Task<FuncionarioDetalheDto?> IncluirAsync(IncluirFuncionarioCommand command, CancellationToken ct = default);
    Task<FuncionarioDetalheDto?> AlterarAsync(AlterarFuncionarioCommand command, CancellationToken ct = default);
    Task<bool> InativarAsync(int id, CancellationToken ct = default);
}

public interface IClienteService
{
    Task<ClienteDetalheDto?> IncluirAsync(SalvarClienteCommand command, CancellationToken ct = default);
    Task<ClienteDetalheDto?> AlterarAsync(int id, SalvarClienteCommand command, CancellationToken ct = default);
    Task<bool> InativarAsync(int id, CancellationToken ct = default);
}

public interface IOrcamentoService
{
    OrcamentoDto Simular(CalcularOrcamentoRequest request);
    Task<OrcamentoDto?> GerarESalvarAsync(CalcularOrcamentoRequest request, CancellationToken ct = default);
}

public interface IProtocoloService
{
    Task<ProtocoloDto?> IncluirAsync(SalvarProtocoloCommand command, CancellationToken ct = default);
    Task<ProtocoloDto?> AlterarAsync(int id, SalvarProtocoloCommand command, CancellationToken ct = default);
    Task<bool> InativarAsync(int id, CancellationToken ct = default);
}

public interface IEscrituraService
{
    Task<EscrituraDetalheDto?> IncluirAsync(SalvarEscrituraCommand command, CancellationToken ct = default);
    Task<EscrituraDetalheDto?> AlterarAsync(int id, SalvarEscrituraCommand command, CancellationToken ct = default);
    Task<EscrituraDetalheDto?> FinalizarLavraturaAsync(FinalizarEscrituraCommand command, CancellationToken ct = default);
    Task<bool> InativarAsync(InativarEscrituraCommand command, CancellationToken ct = default);
    Task<EscrituraDetalheDto?> RegistrarRetiradaDocumentosAsync(RegistrarRetiradaProtocoloCommand command, CancellationToken ct = default);
    Task<DocumentoDigitalizadoDto?> AnexarDocumentoPdfAsync(AnexarDocumentoCommand command, CancellationToken ct = default);
    Task<ArquivoGerado?> BaixarDocumentoPdfAsync(int idDocumento, CancellationToken ct = default);
    Task<bool> RemoverDocumentoPdfAsync(int idDocumento, CancellationToken ct = default);
    Task<ArquivoGerado?> GerarArquivoDoiXmlAsync(string numeroProtocoloOuEscritura, CancellationToken ct = default);
    Task<ArquivoGerado?> GerarCertidaoPdfAsync(string numeroProtocoloOuEscritura, CancellationToken ct = default);
    Task<ArquivoGerado?> GerarProtocoloRetiradaPdfAsync(int idEscritura, CancellationToken ct = default);
}

public interface IBackupService
{
    Task<BackupDto?> AgendarBackupAsync(AgendarBackupCommand command, CancellationToken ct = default);
    Task<BackupDto?> RealizarBackupManualAsync(RealizarBackupManualCommand command, CancellationToken ct = default);
    Task<int> ProcessarBackupsAgendadosAsync(CancellationToken ct = default);
    Task<ArquivoGerado?> BaixarBackupCriptografadoAsync(int idBackup, CancellationToken ct = default);
}
