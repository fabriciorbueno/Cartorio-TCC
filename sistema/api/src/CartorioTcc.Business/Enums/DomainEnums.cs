namespace CartorioTcc.Business.Enums;

/// <summary>
/// Ação de auditoria na tabela de histórico (Inclusão, Alteração, Exclusão).
/// </summary>
public enum AcaoAuditoria
{
    Inclusao = 1,
    Alteracao = 2,
    Exclusao = 3
}

/// <summary>
/// Status geral de registros (Clientes, Funcionários, Perfis: 1 = Ativo, 0 = Inativo).
/// </summary>
public enum StatusRegistro
{
    Inativo = 0,
    Ativo = 1
}

/// <summary>
/// Status da Escritura e etapas do Andamento do Processo (UC-09, UC-12 e tabela Status_Escritura).
/// </summary>
public enum StatusEscritura
{
    Inativa = 0,
    AguardandoDocumentacao = 1,
    EmProcessoDeEscrituracao = 2,
    EmProcessoDeImpressao = 3,
    Finalizada = 4
}

/// <summary>
/// Perfis de acesso dos atores do sistema (UC-01 / Lista de Atores).
/// </summary>
public enum PerfilAcesso
{
    Administrador = 1,
    Tabeliao = 2,
    Escrevente = 3,
    Auxiliar = 4,
    Cliente = 5
}

/// <summary>
/// Tipos de documentos anexados em escrituras digitalizadas (PDF).
/// </summary>
public enum TipoDocumentoDigitalizado
{
    DocumentosApresentados = 1,
    EscrituraDigitalizada = 2,
    CertidaoCasamento = 3,
    CertidaoOutroOrgao = 4,
    ComprovanteResidencia = 5
}

/// <summary>
/// Tipos de relatórios disponíveis no módulo de relatórios (UC-04).
/// </summary>
public enum TipoRelatorioEnum
{
    EscriturasCriadas = 1,
    ProtocolosAtendimento = 2,
    IndiceLivroNotas = 3,
    ClientesCadastrados = 4,
    OrcamentosRealizados = 5,
    AuditoriaSistema = 6
}
