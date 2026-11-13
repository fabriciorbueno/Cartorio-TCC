using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Core.ValueObjects;
using CartorioTcc.Business.Enums;

namespace CartorioTcc.Business.Dtos;

public sealed record EnderecoDto(
    string Cep,
    string Pais,
    string Estado,
    string Cidade,
    string Bairro,
    string Logradouro,
    string Numero,
    string? Complemento = null)
{
    public static EnderecoDto DeDomain(Endereco e) =>
        new(e.Cep, e.Pais, e.Estado, e.Cidade, e.Bairro, e.Logradouro, e.Numero, e.Complemento);

    public Endereco ParaDomain() => new()
    {
        Cep = new string((Cep ?? string.Empty).Where(char.IsDigit).ToArray()),
        Pais = string.IsNullOrWhiteSpace(Pais) ? "Brasil" : Pais.Trim(),
        Estado = (Estado ?? string.Empty).Trim().ToUpperInvariant(),
        Cidade = (Cidade ?? string.Empty).Trim(),
        Bairro = (Bairro ?? string.Empty).Trim(),
        Logradouro = (Logradouro ?? string.Empty).Trim(),
        Numero = (Numero ?? string.Empty).Trim(),
        Complemento = Complemento?.Trim()
    };
}

public sealed record CepConsultaDto(
    string Cep,
    string Logradouro,
    string Bairro,
    string Cidade,
    string Estado,
    string Pais,
    string? Complemento);

// --- UC-01: Permissões, Perfis e Páginas ---
public sealed record PerfilDto(int Id, string NomePerfil, string? Descricao, StatusRegistro Status);

public sealed record PaginaDto(int Id, string NomePagina, string Url, string? Modulo);

public sealed record PermissaoDto(
    int Id,
    int IdPerfil,
    string NomePerfil,
    int IdPagina,
    string NomePagina,
    string UrlPagina,
    string? Modulo,
    bool PodeVisualizar,
    bool PodeIncluir,
    bool PodeEditar,
    bool PodeExcluir,
    StatusRegistro Status);

public sealed record SalvarPermissaoCommand(
    int IdPerfil,
    int IdPagina,
    bool PodeVisualizar = true,
    bool PodeIncluir = true,
    bool PodeEditar = true,
    bool PodeExcluir = true,
    StatusRegistro Status = StatusRegistro.Ativo);

// --- Catálogos: Cartórios e Atos ---
public sealed record CartorioDto(
    int Id,
    string NomeCartorio,
    string CnpjCartorio,
    string? TabeliaoResponsavel,
    string? EnderecoCompleto,
    string? Telefone,
    string? Email,
    string? CodigoCns);

public sealed record AtoDto(
    int Id,
    string TipoAto,
    string DescricaoAto,
    string? ModeloMinutaPadrao,
    bool ExigeImovel,
    bool GeraDoi,
    bool Ativo);

// --- UC-02: Funcionários ---
public sealed record FuncionarioResumoDto(
    int Id,
    string NomeFuncionario,
    string EmailFuncionario,
    string CpfFuncionario,
    string RgFuncionario,
    int IdPerfil,
    string NomePerfil,
    string? Cargo,
    string? TelefoneFuncionario,
    DateOnly? DataAdmissao,
    StatusRegistro StatusFuncionario,
    string CidadeEstado);

public sealed record FuncionarioDetalheDto(
    int Id,
    int IdPerfil,
    string NomePerfil,
    string NomeFuncionario,
    DateOnly DataNascFuncionario,
    DateOnly? DataAdmissao,
    string EmailFuncionario,
    string CpfFuncionario,
    string RgFuncionario,
    string? TelefoneFuncionario,
    string? Cargo,
    StatusRegistro StatusFuncionario,
    EnderecoDto Endereco,
    DateTime DtCriacao,
    DateTime? DtAlteracao);

public sealed record IncluirFuncionarioCommand(
    int IdPerfil,
    string NomeFuncionario,
    DateOnly DataNascFuncionario,
    DateOnly? DataAdmissao,
    string EmailFuncionario,
    string Senha,
    string ConfirmarSenha,
    string CpfFuncionario,
    string RgFuncionario,
    string? TelefoneFuncionario,
    string? Cargo,
    EnderecoDto Endereco,
    StatusRegistro StatusFuncionario = StatusRegistro.Ativo);

public sealed record AlterarFuncionarioCommand(
    int Id,
    int IdPerfil,
    string NomeFuncionario,
    DateOnly DataNascFuncionario,
    DateOnly? DataAdmissao,
    string EmailFuncionario,
    string? NovaSenha,
    string? ConfirmarNovaSenha,
    string CpfFuncionario,
    string RgFuncionario,
    string? TelefoneFuncionario,
    string? Cargo,
    EnderecoDto Endereco,
    StatusRegistro StatusFuncionario = StatusRegistro.Ativo);

public sealed class FiltroFuncionarioDto : FiltroPaginado
{
    public int? IdPerfil { get; set; }
    public StatusRegistro? Status { get; set; }
}

// --- UC-08: Clientes, Cônjuges e Certidão de Casamento ---
public sealed record ConjugeDto(
    int? Id,
    string NomeConjuge,
    string RgConjuge,
    string OrgaoExpedidorConjuge,
    string NacionalidadeConjuge,
    string EstadoOrgaoExpedidorConjuge,
    string CpfConjuge,
    DateOnly DataNascConjuge,
    string EmailConjuge,
    string GeneroConjuge,
    string EstadoCivilConjuge,
    string ProfissaoConjuge,
    string TelefoneConjuge,
    EnderecoDto Endereco);

public sealed record CertidaoCasamentoDto(
    int? Id,
    string CartorioCertidaoCasamento,
    string LivroCertidaoCasamento,
    string FolhasCertidaoCasamento,
    string TermoCertidaoCasamento,
    DateOnly DataCertidaoCasamento,
    string? Averbacoes = null);

public sealed record ClienteResumoDto(
    int Id,
    string NomeCliente,
    string CpfCliente,
    string RgCliente,
    string EmailCliente,
    string TelefoneCliente,
    string EstadoCivilCliente,
    string ProfissaoCliente,
    string CidadeEstado,
    string? NomeConjuge,
    StatusRegistro StatusCliente);

public sealed record ClienteDetalheDto(
    int Id,
    string NomeCliente,
    string RgCliente,
    string OrgaoExpedidorCliente,
    string NacionalidadeCliente,
    string EstadoOrgaoExpedidorCliente,
    string CpfCliente,
    DateOnly DataNascCliente,
    string EmailCliente,
    string GeneroCliente,
    string EstadoCivilCliente,
    string? RegimeBens,
    string ProfissaoCliente,
    string TelefoneCliente,
    StatusRegistro StatusCliente,
    EnderecoDto Endereco,
    ConjugeDto? Conjuge,
    CertidaoCasamentoDto? CertidaoCasamento,
    DateTime DtCriacao,
    DateTime? DtAlteracao);

public sealed record SalvarClienteCommand(
    string NomeCliente,
    string RgCliente,
    string OrgaoExpedidorCliente,
    string NacionalidadeCliente,
    string EstadoOrgaoExpedidorCliente,
    string CpfCliente,
    DateOnly DataNascCliente,
    string EmailCliente,
    string GeneroCliente,
    string EstadoCivilCliente,
    string? RegimeBens,
    string ProfissaoCliente,
    string TelefoneCliente,
    EnderecoDto Endereco,
    StatusRegistro StatusCliente = StatusRegistro.Ativo,
    ConjugeDto? Conjuge = null,
    CertidaoCasamentoDto? CertidaoCasamento = null);

public sealed class FiltroClienteDto : FiltroPaginado
{
    public StatusRegistro? Status { get; set; }
    public string? Cpf { get; set; }
}
