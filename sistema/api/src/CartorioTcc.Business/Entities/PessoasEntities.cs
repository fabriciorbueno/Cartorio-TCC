using CartorioTcc.Business.Core.Entities;
using CartorioTcc.Business.Core.Validations;
using CartorioTcc.Business.Core.ValueObjects;
using CartorioTcc.Business.Enums;

namespace CartorioTcc.Business.Entities;

/// <summary>
/// Tabela Funcionarios (UC-02): Funcionários e usuários autorizados do cartório
/// (Tabelião, Escreventes, Auxiliares e Administrador).
/// </summary>
public class Funcionario : AuditableEntity
{
    public int IdPerfil { get; private set; }
    public Perfil? Perfil { get; private set; }

    public string NomeFuncionario { get; private set; } = string.Empty;
    public DateOnly DataNascFuncionario { get; private set; }
    public DateOnly? DataAdmissao { get; private set; }
    public string EmailFuncionario { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string CpfFuncionario { get; private set; } = string.Empty;
    public string RgFuncionario { get; private set; } = string.Empty;
    public string? TelefoneFuncionario { get; private set; }
    public string? Cargo { get; private set; }
    public StatusRegistro StatusFuncionario { get; private set; } = StatusRegistro.Ativo;
    public Endereco Endereco { get; private set; } = Endereco.Vazio;

    protected Funcionario() { }

    public Funcionario(
        int idPerfil,
        string nomeFuncionario,
        DateOnly dataNascFuncionario,
        DateOnly? dataAdmissao,
        string emailFuncionario,
        string senhaHash,
        string cpfFuncionario,
        string rgFuncionario,
        string? telefoneFuncionario,
        string? cargo,
        Endereco endereco,
        StatusRegistro status = StatusRegistro.Ativo)
    {
        AtualizarDados(
            idPerfil,
            nomeFuncionario,
            dataNascFuncionario,
            dataAdmissao,
            emailFuncionario,
            cpfFuncionario,
            rgFuncionario,
            telefoneFuncionario,
            cargo,
            endereco,
            status);
        SenhaHash = senhaHash;
    }

    public void AtualizarDados(
        int idPerfil,
        string nomeFuncionario,
        DateOnly dataNascFuncionario,
        DateOnly? dataAdmissao,
        string emailFuncionario,
        string cpfFuncionario,
        string rgFuncionario,
        string? telefoneFuncionario,
        string? cargo,
        Endereco endereco,
        StatusRegistro status)
    {
        IdPerfil = idPerfil;
        NomeFuncionario = nomeFuncionario.Trim();
        DataNascFuncionario = dataNascFuncionario;
        DataAdmissao = dataAdmissao;
        EmailFuncionario = emailFuncionario.Trim().ToLowerInvariant();
        CpfFuncionario = Validador.SomenteNumeros(cpfFuncionario);
        RgFuncionario = rgFuncionario.Trim();
        TelefoneFuncionario = Validador.SomenteNumeros(telefoneFuncionario);
        Cargo = cargo?.Trim();
        Endereco = endereco;
        StatusFuncionario = status;
    }

    public void AlterarSenha(string novaSenhaHash) => SenhaHash = novaSenhaHash;

    public void Inativar() => StatusFuncionario = StatusRegistro.Inativo;
    public void Ativar() => StatusFuncionario = StatusRegistro.Ativo;
}

/// <summary>
/// Tabela Clientes (UC-08): Partes interessadas (Solicitantes, Apresentantes, Outorgantes e Outorgados).
/// </summary>
public class Cliente : AuditableEntity
{
    public string NomeCliente { get; private set; } = string.Empty;
    public string RgCliente { get; private set; } = string.Empty;
    public string OrgaoExpedidorCliente { get; private set; } = string.Empty;
    public string NacionalidadeCliente { get; private set; } = "Brasileiro(a)";
    public string EstadoOrgaoExpedidorCliente { get; private set; } = "SP";
    public string CpfCliente { get; private set; } = string.Empty;
    public DateOnly DataNascCliente { get; private set; }
    public string EmailCliente { get; private set; } = string.Empty;
    public string GeneroCliente { get; private set; } = "M";
    public string EstadoCivilCliente { get; private set; } = "Solteiro(a)";
    public string? RegimeBens { get; private set; }
    public string ProfissaoCliente { get; private set; } = string.Empty;
    public string TelefoneCliente { get; private set; } = string.Empty;
    public StatusRegistro StatusCliente { get; private set; } = StatusRegistro.Ativo;
    public Endereco Endereco { get; private set; } = Endereco.Vazio;

    public Conjuge? Conjuge { get; private set; }
    public CertidaoCasamento? CertidaoCasamento { get; private set; }

    protected Cliente() { }

    public Cliente(
        string nomeCliente,
        string rgCliente,
        string orgaoExpedidorCliente,
        string nacionalidadeCliente,
        string estadoOrgaoExpedidorCliente,
        string cpfCliente,
        DateOnly dataNascCliente,
        string emailCliente,
        string generoCliente,
        string estadoCivilCliente,
        string? regimeBens,
        string profissaoCliente,
        string telefoneCliente,
        Endereco endereco,
        StatusRegistro status = StatusRegistro.Ativo)
    {
        Atualizar(
            nomeCliente,
            rgCliente,
            orgaoExpedidorCliente,
            nacionalidadeCliente,
            estadoOrgaoExpedidorCliente,
            cpfCliente,
            dataNascCliente,
            emailCliente,
            generoCliente,
            estadoCivilCliente,
            regimeBens,
            profissaoCliente,
            telefoneCliente,
            endereco,
            status);
    }

    public void Atualizar(
        string nomeCliente,
        string rgCliente,
        string orgaoExpedidorCliente,
        string nacionalidadeCliente,
        string estadoOrgaoExpedidorCliente,
        string cpfCliente,
        DateOnly dataNascCliente,
        string emailCliente,
        string generoCliente,
        string estadoCivilCliente,
        string? regimeBens,
        string profissaoCliente,
        string telefoneCliente,
        Endereco endereco,
        StatusRegistro status)
    {
        NomeCliente = nomeCliente.Trim();
        RgCliente = rgCliente.Trim();
        OrgaoExpedidorCliente = orgaoExpedidorCliente.Trim().ToUpperInvariant();
        NacionalidadeCliente = string.IsNullOrWhiteSpace(nacionalidadeCliente) ? "Brasileiro(a)" : nacionalidadeCliente.Trim();
        EstadoOrgaoExpedidorCliente = estadoOrgaoExpedidorCliente.Trim().ToUpperInvariant();
        CpfCliente = Validador.SomenteNumeros(cpfCliente);
        DataNascCliente = dataNascCliente;
        EmailCliente = emailCliente.Trim().ToLowerInvariant();
        GeneroCliente = string.IsNullOrWhiteSpace(generoCliente) ? "M" : generoCliente.Trim().ToUpperInvariant()[..1];
        EstadoCivilCliente = estadoCivilCliente.Trim();
        RegimeBens = regimeBens?.Trim();
        ProfissaoCliente = profissaoCliente.Trim();
        TelefoneCliente = Validador.SomenteNumeros(telefoneCliente);
        Endereco = endereco;
        StatusCliente = status;
    }

    public void DefinirConjuge(Conjuge? conjuge) => Conjuge = conjuge;

    public void DefinirCertidaoCasamento(CertidaoCasamento? certidao) => CertidaoCasamento = certidao;

    public void Inativar() => StatusCliente = StatusRegistro.Inativo;
    public void Ativar() => StatusCliente = StatusRegistro.Ativo;
}

/// <summary>
/// Tabela Conjuges (UC-08 / UC-09): Dados do cônjuge vinculado ao cliente.
/// </summary>
public class Conjuge : AuditableEntity
{
    public int IdCliente { get; private set; }
    public Cliente? Cliente { get; private set; }

    public string NomeConjuge { get; private set; } = string.Empty;
    public string RgConjuge { get; private set; } = string.Empty;
    public string OrgaoExpedidorConjuge { get; private set; } = string.Empty;
    public string NacionalidadeConjuge { get; private set; } = "Brasileiro(a)";
    public string EstadoOrgaoExpedidorConjuge { get; private set; } = "SP";
    public string CpfConjuge { get; private set; } = string.Empty;
    public DateOnly DataNascConjuge { get; private set; }
    public string EmailConjuge { get; private set; } = string.Empty;
    public string GeneroConjuge { get; private set; } = "F";
    public string EstadoCivilConjuge { get; private set; } = "Casado(a)";
    public string ProfissaoConjuge { get; private set; } = string.Empty;
    public string TelefoneConjuge { get; private set; } = string.Empty;
    public StatusRegistro StatusConjuge { get; private set; } = StatusRegistro.Ativo;
    public Endereco Endereco { get; private set; } = Endereco.Vazio;

    protected Conjuge() { }

    public Conjuge(
        int idCliente,
        string nomeConjuge,
        string rgConjuge,
        string orgaoExpedidorConjuge,
        string nacionalidadeConjuge,
        string estadoOrgaoExpedidorConjuge,
        string cpfConjuge,
        DateOnly dataNascConjuge,
        string emailConjuge,
        string generoConjuge,
        string estadoCivilConjuge,
        string profissaoConjuge,
        string telefoneConjuge,
        Endereco endereco)
    {
        IdCliente = idCliente;
        Atualizar(
            nomeConjuge,
            rgConjuge,
            orgaoExpedidorConjuge,
            nacionalidadeConjuge,
            estadoOrgaoExpedidorConjuge,
            cpfConjuge,
            dataNascConjuge,
            emailConjuge,
            generoConjuge,
            estadoCivilConjuge,
            profissaoConjuge,
            telefoneConjuge,
            endereco);
    }

    public void Atualizar(
        string nomeConjuge,
        string rgConjuge,
        string orgaoExpedidorConjuge,
        string nacionalidadeConjuge,
        string estadoOrgaoExpedidorConjuge,
        string cpfConjuge,
        DateOnly dataNascConjuge,
        string emailConjuge,
        string generoConjuge,
        string estadoCivilConjuge,
        string profissaoConjuge,
        string telefoneConjuge,
        Endereco endereco)
    {
        NomeConjuge = nomeConjuge.Trim();
        RgConjuge = rgConjuge.Trim();
        OrgaoExpedidorConjuge = orgaoExpedidorConjuge.Trim().ToUpperInvariant();
        NacionalidadeConjuge = string.IsNullOrWhiteSpace(nacionalidadeConjuge) ? "Brasileiro(a)" : nacionalidadeConjuge.Trim();
        EstadoOrgaoExpedidorConjuge = estadoOrgaoExpedidorConjuge.Trim().ToUpperInvariant();
        CpfConjuge = Validador.SomenteNumeros(cpfConjuge);
        DataNascConjuge = dataNascConjuge;
        EmailConjuge = emailConjuge.Trim().ToLowerInvariant();
        GeneroConjuge = string.IsNullOrWhiteSpace(generoConjuge) ? "F" : generoConjuge.Trim().ToUpperInvariant()[..1];
        EstadoCivilConjuge = estadoCivilConjuge.Trim();
        ProfissaoConjuge = profissaoConjuge.Trim();
        TelefoneConjuge = Validador.SomenteNumeros(telefoneConjuge);
        Endereco = endereco;
    }
}

/// <summary>
/// Tabela Certidoes_Casamentos: Certidão de casamento associada a cliente e cônjuge.
/// </summary>
public class CertidaoCasamento : AuditableEntity
{
    public int IdCliente { get; private set; }
    public Cliente? Cliente { get; private set; }

    public int? IdConjuge { get; private set; }
    public Conjuge? Conjuge { get; private set; }

    public string CartorioCertidaoCasamento { get; private set; } = string.Empty;
    public string LivroCertidaoCasamento { get; private set; } = string.Empty;
    public string FolhasCertidaoCasamento { get; private set; } = string.Empty;
    public string TermoCertidaoCasamento { get; private set; } = string.Empty;
    public DateOnly DataCertidaoCasamento { get; private set; }
    public string? Averbacoes { get; private set; }

    protected CertidaoCasamento() { }

    public CertidaoCasamento(
        int idCliente,
        int? idConjuge,
        string cartorio,
        string livro,
        string folhas,
        string termo,
        DateOnly dataCertidao,
        string? averbacoes = null)
    {
        IdCliente = idCliente;
        Atualizar(idConjuge, cartorio, livro, folhas, termo, dataCertidao, averbacoes);
    }

    public void Atualizar(
        int? idConjuge,
        string cartorio,
        string livro,
        string folhas,
        string termo,
        DateOnly dataCertidao,
        string? averbacoes = null)
    {
        IdConjuge = idConjuge;
        CartorioCertidaoCasamento = cartorio.Trim();
        LivroCertidaoCasamento = livro.Trim();
        FolhasCertidaoCasamento = folhas.Trim();
        TermoCertidaoCasamento = termo.Trim();
        DataCertidaoCasamento = dataCertidao;
        Averbacoes = averbacoes?.Trim();
    }
}

/// <summary>
/// Tabela Certidoes_Outros_Orgaos: Certidões diversas (matrícula, negativas, etc.) vinculadas à escritura.
/// </summary>
public class CertidaoOutroOrgao : AuditableEntity
{
    public string CartorioCertidaoOutroOrgao { get; private set; } = string.Empty;
    public string LivroCertidaoOutroOrgao { get; private set; } = string.Empty;
    public string FolhasCertidaoOutroOrgao { get; private set; } = string.Empty;
    public string TermoCertidaoOutroOrgao { get; private set; } = string.Empty;
    public DateOnly DataCertidaoOutroOrgao { get; private set; }

    protected CertidaoOutroOrgao() { }

    public CertidaoOutroOrgao(string cartorio, string livro, string folhas, string termo, DateOnly dataCertidao)
    {
        Atualizar(cartorio, livro, folhas, termo, dataCertidao);
    }

    public void Atualizar(string cartorio, string livro, string folhas, string termo, DateOnly dataCertidao)
    {
        CartorioCertidaoOutroOrgao = cartorio.Trim();
        LivroCertidaoOutroOrgao = livro.Trim();
        FolhasCertidaoOutroOrgao = folhas.Trim();
        TermoCertidaoOutroOrgao = termo.Trim();
        DataCertidaoOutroOrgao = dataCertidao;
    }
}
