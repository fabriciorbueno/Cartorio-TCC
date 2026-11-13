using CartorioTcc.Business.Core.Entities;
using CartorioTcc.Business.Core.ValueObjects;
using CartorioTcc.Business.Enums;

namespace CartorioTcc.Business.Entities;

/// <summary>
/// Tabela Imoveis (UC-09): Dados do imóvel objeto da escritura (matrícula, valor venal, prefeitura, área e endereço).
/// </summary>
public class Imovel : AuditableEntity
{
    public int? IdCliente { get; private set; }
    public Cliente? Cliente { get; private set; }

    public string NumeroRegistro { get; private set; } = string.Empty;
    public decimal ValorVenal { get; private set; }
    public string NomePrefeitura { get; private set; } = string.Empty;
    public string? DescricaoImovel { get; private set; }
    public decimal ValorImovel { get; private set; }
    public decimal? AreaMetrosQuadrados { get; private set; }
    public decimal? ValorPorMetroQuadrado { get; private set; }
    public string? InscricaoMunicipal { get; private set; }
    public string? CartorioRegistroImoveis { get; private set; }
    public Endereco Endereco { get; private set; } = Endereco.Vazio;

    protected Imovel() { }

    public Imovel(
        int? idCliente,
        string numeroRegistro,
        decimal valorVenal,
        string nomePrefeitura,
        string? descricaoImovel,
        decimal valorImovel,
        Endereco endereco,
        decimal? areaMetrosQuadrados = null,
        string? inscricaoMunicipal = null,
        string? cartorioRegistroImoveis = null)
    {
        Atualizar(
            idCliente,
            numeroRegistro,
            valorVenal,
            nomePrefeitura,
            descricaoImovel,
            valorImovel,
            endereco,
            areaMetrosQuadrados,
            inscricaoMunicipal,
            cartorioRegistroImoveis);
    }

    public void Atualizar(
        int? idCliente,
        string numeroRegistro,
        decimal valorVenal,
        string nomePrefeitura,
        string? descricaoImovel,
        decimal valorImovel,
        Endereco endereco,
        decimal? areaMetrosQuadrados = null,
        string? inscricaoMunicipal = null,
        string? cartorioRegistroImoveis = null)
    {
        IdCliente = idCliente;
        NumeroRegistro = numeroRegistro.Trim();
        ValorVenal = valorVenal;
        NomePrefeitura = nomePrefeitura.Trim();
        DescricaoImovel = descricaoImovel?.Trim();
        ValorImovel = valorImovel;
        AreaMetrosQuadrados = areaMetrosQuadrados;
        ValorPorMetroQuadrado = areaMetrosQuadrados is > 0
            ? Math.Round(valorImovel / areaMetrosQuadrados.Value, 2, MidpointRounding.AwayFromZero)
            : null;
        InscricaoMunicipal = inscricaoMunicipal?.Trim();
        CartorioRegistroImoveis = cartorioRegistroImoveis?.Trim();
        Endereco = (endereco ?? Endereco.Vazio).Clonar();
    }
}

/// <summary>
/// Tabela Orcamentos (UC-06): Simulação e registro de orçamento/emolumentos conforme Tabela de Custas CNB/SP.
/// </summary>
public class Orcamento : AuditableEntity
{
    public int? IdCliente { get; private set; }
    public Cliente? Cliente { get; private set; }

    public decimal ValorBaseAto { get; private set; }
    public bool Oneroso { get; private set; } = true;
    public bool Hipoteca { get; private set; }
    public bool Ufesp2500 { get; private set; }
    public bool Salarios30 { get; private set; }
    public bool CertidaoInclusa { get; private set; } = true;
    public decimal OutrosValores { get; private set; }

    // Parcelas detalhadas segundo Tabela de Custas CNB/SP (Lei Estadual SP 11.331/2002)
    public string FaixaDescricao { get; private set; } = string.Empty;
    public decimal EmolumentosTabelionato { get; private set; }
    public decimal EmolumentosEstado { get; private set; }
    public decimal ContribuicaoIpesp { get; private set; }
    public decimal FundoRegistroCivil { get; private set; }
    public decimal TaxaTribunalJustica { get; private set; }
    public decimal FundoMinisterioPublico { get; private set; }
    public decimal IssqnEstimado { get; private set; }
    public decimal ValorCertidao { get; private set; }
    public decimal PrevisaoRegistroImoveis { get; private set; }
    public decimal ValorTotalOrcamento { get; private set; }

    protected Orcamento() { }

    public Orcamento(
        int? idCliente,
        decimal valorBaseAto,
        bool oneroso,
        bool hipoteca,
        bool ufesp2500,
        bool salarios30,
        bool certidaoInclusa,
        decimal outrosValores,
        string faixaDescricao,
        decimal emolumentosTabelionato,
        decimal emolumentosEstado,
        decimal contribuicaoIpesp,
        decimal fundoRegistroCivil,
        decimal taxaTribunalJustica,
        decimal fundoMinisterioPublico,
        decimal issqnEstimado,
        decimal valorCertidao,
        decimal previsaoRegistroImoveis,
        decimal valorTotalOrcamento)
    {
        IdCliente = idCliente;
        ValorBaseAto = valorBaseAto;
        Oneroso = oneroso;
        Hipoteca = hipoteca;
        Ufesp2500 = ufesp2500;
        Salarios30 = salarios30;
        CertidaoInclusa = certidaoInclusa;
        OutrosValores = outrosValores;
        FaixaDescricao = faixaDescricao;
        EmolumentosTabelionato = emolumentosTabelionato;
        EmolumentosEstado = emolumentosEstado;
        ContribuicaoIpesp = contribuicaoIpesp;
        FundoRegistroCivil = fundoRegistroCivil;
        TaxaTribunalJustica = taxaTribunalJustica;
        FundoMinisterioPublico = fundoMinisterioPublico;
        IssqnEstimado = issqnEstimado;
        ValorCertidao = valorCertidao;
        PrevisaoRegistroImoveis = previsaoRegistroImoveis;
        ValorTotalOrcamento = valorTotalOrcamento;
    }
}

/// <summary>
/// Tabela Protocolos (UC-07 / UC-12): Protocolo de atendimento vinculado ao cliente apresentante e ao ato solicitado.
/// </summary>
public class Protocolo : AuditableEntity
{
    /// <summary>Regra UC-07: preenchimento automático, alfanumérico até 10 caracteres (ex.: 2026000001).</summary>
    public string NumeroProtocolo { get; private set; } = string.Empty;

    public int IdCliente { get; private set; }
    public Cliente? Cliente { get; private set; }

    public int? IdAto { get; private set; }
    public Ato? Ato { get; private set; }

    public int? IdFuncionario { get; private set; }
    public Funcionario? Funcionario { get; private set; }

    public int? IdOrcamento { get; private set; }
    public Orcamento? Orcamento { get; private set; }

    public string? NaturezaPedido { get; private set; }
    public string? DocumentosApresentados { get; private set; }
    public string? DocumentosFaltantes { get; private set; }
    public decimal? ValorOrcamentoEstimado { get; private set; }
    public string? Observacoes { get; private set; }
    public StatusEscritura StatusAndamento { get; private set; } = StatusEscritura.AguardandoDocumentacao;
    public StatusRegistro StatusProtocolo { get; private set; } = StatusRegistro.Ativo;

    protected Protocolo() { }

    public Protocolo(
        string numeroProtocolo,
        int idCliente,
        int? idAto = null,
        int? idFuncionario = null,
        int? idOrcamento = null,
        string? naturezaPedido = null,
        string? documentosApresentados = null,
        string? documentosFaltantes = null,
        decimal? valorOrcamentoEstimado = null,
        string? observacoes = null)
    {
        NumeroProtocolo = numeroProtocolo.Trim().ToUpperInvariant();
        Atualizar(
            idCliente,
            idAto,
            idFuncionario,
            idOrcamento,
            naturezaPedido,
            documentosApresentados,
            documentosFaltantes,
            valorOrcamentoEstimado,
            observacoes,
            StatusEscritura.AguardandoDocumentacao);
    }

    public void Atualizar(
        int idCliente,
        int? idAto,
        int? idFuncionario,
        int? idOrcamento,
        string? naturezaPedido,
        string? documentosApresentados,
        string? documentosFaltantes,
        decimal? valorOrcamentoEstimado,
        string? observacoes,
        StatusEscritura statusAndamento)
    {
        IdCliente = idCliente;
        IdAto = idAto;
        IdFuncionario = idFuncionario;
        IdOrcamento = idOrcamento;
        NaturezaPedido = naturezaPedido?.Trim();
        DocumentosApresentados = documentosApresentados?.Trim();
        DocumentosFaltantes = documentosFaltantes?.Trim();
        ValorOrcamentoEstimado = valorOrcamentoEstimado;
        Observacoes = observacoes?.Trim();
        StatusAndamento = statusAndamento;
    }

    public void AlterarStatusAndamento(StatusEscritura novoStatus) => StatusAndamento = novoStatus;

    public void Inativar()
    {
        StatusProtocolo = StatusRegistro.Inativo;
        StatusAndamento = StatusEscritura.Inativa;
    }
}

/// <summary>
/// Tabela Escrituras (UC-09, UC-10, UC-11, UC-12): Escritura pública lavrada no tabelionato.
/// Ao ser finalizada (lavrada), gera automaticamente o registro no Índice e disponibiliza exportação DOI e Certidão.
/// </summary>
public class Escritura : AuditableEntity
{
    public int IdCartorio { get; private set; }
    public Cartorio? Cartorio { get; private set; }

    public int IdOutorgante { get; private set; }
    public Cliente? Outorgante { get; private set; }

    public int IdOutorgado { get; private set; }
    public Cliente? Outorgado { get; private set; }

    public int IdFuncionario { get; private set; }
    public Funcionario? Funcionario { get; private set; }

    public int IdProtocolo { get; private set; }
    public Protocolo? Protocolo { get; private set; }

    public int IdAto { get; private set; }
    public Ato? Ato { get; private set; }

    public int? IdImovel { get; private set; }
    public Imovel? Imovel { get; private set; }

    public int? IdCertidaoOutroOrgao { get; private set; }
    public CertidaoOutroOrgao? CertidaoOutroOrgao { get; private set; }

    public string NumeroEscritura { get; private set; } = string.Empty;
    public int NumeroLivro { get; private set; }
    public int FolhaInicial { get; private set; }
    public int FolhaFinal { get; private set; }
    public decimal ValorOperacao { get; private set; }
    public string? TextoMinuta { get; private set; }

    public StatusEscritura StatusEscritura { get; private set; } = StatusEscritura.EmProcessoDeEscrituracao;
    public string? MotivoInativacao { get; private set; }

    public DateTime DataInicioProcesso { get; private set; }
    public DateTime? DataLavratura { get; private set; }
    public DateTime? DataFimProcesso { get; private set; }
    public DateTime? DataEnvioDoi { get; private set; }
    public bool DocumentosRetiradosPeloCliente { get; private set; }
    public DateTime? DataRetiradaDocumentos { get; private set; }
    public string? ObservacoesRetirada { get; private set; }

    private readonly List<DocumentoDigitalizado> _documentos = [];
    public IReadOnlyCollection<DocumentoDigitalizado> Documentos => _documentos.AsReadOnly();

    protected Escritura() { }

    public Escritura(
        int idCartorio,
        int idOutorgante,
        int idOutorgado,
        int idFuncionario,
        int idProtocolo,
        int idAto,
        int? idImovel,
        int? idCertidaoOutroOrgao,
        string numeroEscritura,
        int numeroLivro,
        int folhaInicial,
        int folhaFinal,
        decimal valorOperacao,
        string? textoMinuta,
        DateTime dataInicioProcesso,
        StatusEscritura statusInicial = StatusEscritura.EmProcessoDeEscrituracao)
    {
        IdCartorio = idCartorio;
        IdOutorgante = idOutorgante;
        IdOutorgado = idOutorgado;
        IdFuncionario = idFuncionario;
        IdProtocolo = idProtocolo;
        IdAto = idAto;
        IdImovel = idImovel;
        IdCertidaoOutroOrgao = idCertidaoOutroOrgao;
        NumeroEscritura = numeroEscritura.Trim();
        NumeroLivro = numeroLivro;
        FolhaInicial = folhaInicial;
        FolhaFinal = folhaFinal;
        ValorOperacao = valorOperacao;
        TextoMinuta = textoMinuta;
        DataInicioProcesso = dataInicioProcesso;
        StatusEscritura = statusInicial;
    }

    public bool EstaFinalizada => StatusEscritura == StatusEscritura.Finalizada;

    public void Atualizar(
        int idCartorio,
        int idOutorgante,
        int idOutorgado,
        int idFuncionario,
        int idProtocolo,
        int idAto,
        int? idImovel,
        int? idCertidaoOutroOrgao,
        int numeroLivro,
        int folhaInicial,
        int folhaFinal,
        decimal valorOperacao,
        string? textoMinuta,
        StatusEscritura status)
    {
        IdCartorio = idCartorio;
        IdOutorgante = idOutorgante;
        IdOutorgado = idOutorgado;
        IdFuncionario = idFuncionario;
        IdProtocolo = idProtocolo;
        IdAto = idAto;
        IdImovel = idImovel;
        IdCertidaoOutroOrgao = idCertidaoOutroOrgao;
        NumeroLivro = numeroLivro;
        FolhaInicial = folhaInicial;
        FolhaFinal = folhaFinal;
        ValorOperacao = valorOperacao;
        TextoMinuta = textoMinuta;
        StatusEscritura = status;
    }

    public void FinalizarLavratura(DateTime dataLavratura, string? textoMinutaDefinitivo = null)
    {
        if (!string.IsNullOrWhiteSpace(textoMinutaDefinitivo))
            TextoMinuta = textoMinutaDefinitivo;

        StatusEscritura = StatusEscritura.Finalizada;
        DataLavratura = dataLavratura;
        DataFimProcesso ??= dataLavratura;
    }

    public void Inativar(string motivo)
    {
        StatusEscritura = StatusEscritura.Inativa;
        MotivoInativacao = motivo.Trim();
    }

    public void RegistrarEnvioDoi(DateTime dataEnvio) => DataEnvioDoi = dataEnvio;

    public void RegistrarRetiradaDocumentos(DateTime dataRetirada, string? observacoes = null)
    {
        DocumentosRetiradosPeloCliente = true;
        DataRetiradaDocumentos = dataRetirada;
        DataFimProcesso = dataRetirada;
        ObservacoesRetirada = observacoes?.Trim();
    }

    public void AdicionarDocumento(DocumentoDigitalizado documento) => _documentos.Add(documento);
}

/// <summary>
/// Documentos digitalizados (PDF) anexados à escritura e/ou cliente (aba Digitalizações do UC-09 e pré-condição do UC-11).
/// </summary>
public class DocumentoDigitalizado : AuditableEntity
{
    public int? IdEscritura { get; private set; }
    public Escritura? Escritura { get; private set; }

    public int? IdCliente { get; private set; }
    public Cliente? Cliente { get; private set; }

    public TipoDocumentoDigitalizado TipoDocumento { get; private set; }
    public string NomeArquivo { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/pdf";
    public long TamanhoBytes { get; private set; }
    public string HashSha256 { get; private set; } = string.Empty;
    public byte[] ConteudoPdf { get; private set; } = [];
    public string? Observacao { get; private set; }

    protected DocumentoDigitalizado() { }

    public DocumentoDigitalizado(
        int? idEscritura,
        int? idCliente,
        TipoDocumentoDigitalizado tipoDocumento,
        string nomeArquivo,
        string contentType,
        byte[] conteudoPdf,
        string hashSha256,
        string? observacao = null)
    {
        IdEscritura = idEscritura;
        IdCliente = idCliente;
        TipoDocumento = tipoDocumento;
        NomeArquivo = nomeArquivo.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType.Trim();
        ConteudoPdf = conteudoPdf;
        TamanhoBytes = conteudoPdf.LongLength;
        HashSha256 = hashSha256;
        Observacao = observacao?.Trim();
    }
}

/// <summary>
/// Tabela Backups (UC-03 / UC-05): Controle de backups criptografados (agendados e manuais).
/// </summary>
public class BackupRegistro : AuditableEntity
{
    public DateTime DataBackup { get; private set; }
    public DateTime? DataAgendada { get; private set; }
    public string DescricaoBackup { get; private set; } = string.Empty;
    public int? IdFuncionario { get; private set; }
    public Funcionario? Funcionario { get; private set; }
    public bool Manual { get; private set; }
    public bool Criptografado { get; private set; } = true;
    public bool Executado { get; private set; }
    public string NomeArquivoGerado { get; private set; } = string.Empty;
    public long TamanhoBytes { get; private set; }
    public string HashIntegridade { get; private set; } = string.Empty;
    public byte[]? ConteudoCriptografado { get; private set; }

    protected BackupRegistro() { }

    public BackupRegistro(
        DateTime dataBackup,
        DateTime? dataAgendada,
        string descricaoBackup,
        int? idFuncionario,
        bool manual,
        bool executado,
        string nomeArquivoGerado,
        long tamanhoBytes,
        string hashIntegridade,
        byte[]? conteudoCriptografado = null)
    {
        DataBackup = dataBackup;
        DataAgendada = dataAgendada;
        DescricaoBackup = descricaoBackup.Trim();
        IdFuncionario = idFuncionario;
        Manual = manual;
        Criptografado = true;
        Executado = executado;
        NomeArquivoGerado = nomeArquivoGerado;
        TamanhoBytes = tamanhoBytes;
        HashIntegridade = hashIntegridade;
        ConteudoCriptografado = conteudoCriptografado;
    }

    public void MarcarExecutado(DateTime dataExecucao, string nomeArquivo, long tamanhoBytes, string hash, byte[] conteudo)
    {
        DataBackup = dataExecucao;
        Executado = true;
        NomeArquivoGerado = nomeArquivo;
        TamanhoBytes = tamanhoBytes;
        HashIntegridade = hash;
        ConteudoCriptografado = conteudo;
    }
}

/// <summary>
/// Tabela de Histórico/Auditoria padronizada (item 2.C da arquitetura e RN do UC-04).
/// </summary>
public class HistoricoAuditoria : Entity
{
    public string Tabela { get; private set; } = string.Empty;
    public string ChavePrimaria { get; private set; } = string.Empty;
    public AcaoAuditoria Acao { get; private set; }
    public string LoginUsuario { get; private set; } = string.Empty;
    public DateTime DataOcorrencia { get; private set; }
    public string? ValoresAntigosJson { get; private set; }
    public string? ValoresNovosJson { get; private set; }

    protected HistoricoAuditoria() { }

    public HistoricoAuditoria(
        string tabela,
        string chavePrimaria,
        AcaoAuditoria acao,
        string loginUsuario,
        DateTime dataOcorrencia,
        string? valoresAntigosJson,
        string? valoresNovosJson)
    {
        Tabela = tabela;
        ChavePrimaria = chavePrimaria;
        Acao = acao;
        LoginUsuario = loginUsuario;
        DataOcorrencia = dataOcorrencia;
        ValoresAntigosJson = valoresAntigosJson;
        ValoresNovosJson = valoresNovosJson;
    }
}
