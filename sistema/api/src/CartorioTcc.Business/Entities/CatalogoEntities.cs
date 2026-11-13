using CartorioTcc.Business.Core.Entities;
using CartorioTcc.Business.Enums;

namespace CartorioTcc.Business.Entities;

/// <summary>
/// Tabela Atos: naturezas de atos notariais (Compra e Venda, Permuta, Doação, Dação em Pagamento,
/// Confissão de Dívida, Inventário e Partilha, Divórcio, Procuração Pública, etc.).
/// </summary>
public class Ato : AuditableEntity
{
    public string TipoAto { get; private set; } = string.Empty;
    public string DescricaoAto { get; private set; } = string.Empty;
    public string? ModeloMinutaPadrao { get; private set; }
    public bool ExigeImovel { get; private set; } = true;
    public bool GeraDoi { get; private set; } = true;
    public bool Ativo { get; private set; } = true;

    protected Ato() { }

    public Ato(string tipoAto, string descricaoAto, string? modeloMinutaPadrao = null, bool exigeImovel = true, bool geraDoi = true)
    {
        Atualizar(tipoAto, descricaoAto, modeloMinutaPadrao, exigeImovel, geraDoi);
    }

    public void Atualizar(string tipoAto, string descricaoAto, string? modeloMinutaPadrao = null, bool exigeImovel = true, bool geraDoi = true)
    {
        TipoAto = tipoAto.Trim();
        DescricaoAto = descricaoAto.Trim();
        ModeloMinutaPadrao = modeloMinutaPadrao;
        ExigeImovel = exigeImovel;
        GeraDoi = geraDoi;
    }

    public void AlterarStatus(bool ativo) => Ativo = ativo;
}

/// <summary>
/// Tabela Cartorios: identificação do cartório (ex.: 2º Tabelionato de Notas de Campinas).
/// </summary>
public class Cartorio : AuditableEntity
{
    public string NomeCartorio { get; private set; } = string.Empty;
    public string CnpjCartorio { get; private set; } = string.Empty;
    public string? TabeliaoResponsavel { get; private set; }
    public string? EnderecoCompleto { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? CodigoCns { get; private set; }

    protected Cartorio() { }

    public Cartorio(
        string nomeCartorio,
        string cnpjCartorio,
        string? tabeliaoResponsavel = null,
        string? enderecoCompleto = null,
        string? telefone = null,
        string? email = null,
        string? codigoCns = null)
    {
        Atualizar(nomeCartorio, cnpjCartorio, tabeliaoResponsavel, enderecoCompleto, telefone, email, codigoCns);
    }

    public void Atualizar(
        string nomeCartorio,
        string cnpjCartorio,
        string? tabeliaoResponsavel = null,
        string? enderecoCompleto = null,
        string? telefone = null,
        string? email = null,
        string? codigoCns = null)
    {
        NomeCartorio = nomeCartorio.Trim();
        CnpjCartorio = cnpjCartorio.Trim();
        TabeliaoResponsavel = tabeliaoResponsavel?.Trim();
        EnderecoCompleto = enderecoCompleto?.Trim();
        Telefone = telefone?.Trim();
        Email = email?.Trim();
        CodigoCns = codigoCns?.Trim();
    }
}

/// <summary>
/// Tabela Perfis (UC-01): Perfis de acesso (Administrador, Tabelião, Escrevente, Auxiliar, Cliente).
/// </summary>
public class Perfil : AuditableEntity
{
    public string NomePerfil { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public StatusRegistro Status { get; private set; } = StatusRegistro.Ativo;

    private readonly List<PaginaPerfil> _paginasPerfis = [];
    public IReadOnlyCollection<PaginaPerfil> PaginasPerfis => _paginasPerfis.AsReadOnly();

    protected Perfil() { }

    public Perfil(string nomePerfil, string? descricao = null, StatusRegistro status = StatusRegistro.Ativo)
    {
        NomePerfil = nomePerfil.Trim();
        Descricao = descricao?.Trim();
        Status = status;
    }

    public void Atualizar(string nomePerfil, string? descricao, StatusRegistro status)
    {
        NomePerfil = nomePerfil.Trim();
        Descricao = descricao?.Trim();
        Status = status;
    }
}

/// <summary>
/// Tabela Paginas (UC-01): Páginas/módulos do sistema vinculáveis aos perfis.
/// </summary>
public class Pagina : AuditableEntity
{
    public string NomePagina { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string? Modulo { get; private set; }

    protected Pagina() { }

    public Pagina(string nomePagina, string url, string? modulo = null)
    {
        NomePagina = nomePagina.Trim();
        Url = url.Trim();
        Modulo = modulo?.Trim();
    }

    public void Atualizar(string nomePagina, string url, string? modulo = null)
    {
        NomePagina = nomePagina.Trim();
        Url = url.Trim();
        Modulo = modulo?.Trim();
    }
}

/// <summary>
/// Tabela Paginas_Perfis (UC-01): Associação entre Perfil e Página com permissões granulares.
/// </summary>
public class PaginaPerfil : AuditableEntity
{
    public int IdPerfil { get; private set; }
    public Perfil? Perfil { get; private set; }

    public int IdPagina { get; private set; }
    public Pagina? Pagina { get; private set; }

    public bool PodeVisualizar { get; private set; } = true;
    public bool PodeIncluir { get; private set; } = true;
    public bool PodeEditar { get; private set; } = true;
    public bool PodeExcluir { get; private set; } = true;
    public StatusRegistro Status { get; private set; } = StatusRegistro.Ativo;

    protected PaginaPerfil() { }

    public PaginaPerfil(
        int idPerfil,
        int idPagina,
        bool podeVisualizar = true,
        bool podeIncluir = true,
        bool podeEditar = true,
        bool podeExcluir = true)
    {
        Atualizar(idPerfil, idPagina, podeVisualizar, podeIncluir, podeEditar, podeExcluir, StatusRegistro.Ativo);
    }

    public void Atualizar(
        int idPerfil,
        int idPagina,
        bool podeVisualizar,
        bool podeIncluir,
        bool podeEditar,
        bool podeExcluir,
        StatusRegistro status)
    {
        IdPerfil = idPerfil;
        IdPagina = idPagina;
        PodeVisualizar = podeVisualizar;
        PodeIncluir = podeIncluir;
        PodeEditar = podeEditar;
        PodeExcluir = podeExcluir;
        Status = status;
    }
}

/// <summary>
/// Tabela Tipos_Relatorios (UC-04): Catálogo de relatórios disponíveis.
/// </summary>
public class TipoRelatorio : AuditableEntity
{
    public string NomeTipoRelatorio { get; private set; } = string.Empty;
    public string DescricaoRelatorio { get; private set; } = string.Empty;
    public TipoRelatorioEnum Codigo { get; private set; }

    protected TipoRelatorio() { }

    public TipoRelatorio(TipoRelatorioEnum codigo, string nomeTipoRelatorio, string descricaoRelatorio)
    {
        Codigo = codigo;
        NomeTipoRelatorio = nomeTipoRelatorio.Trim();
        DescricaoRelatorio = descricaoRelatorio.Trim();
    }
}
