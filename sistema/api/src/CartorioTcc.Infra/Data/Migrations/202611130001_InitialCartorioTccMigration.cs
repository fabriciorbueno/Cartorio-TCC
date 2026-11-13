using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using CartorioTcc.Infra.Data.Context;

namespace CartorioTcc.Infra.Data.Migrations;

[DbContext(typeof(CartorioTccContext))]
[Migration("202611130001_InitialCartorioTccMigration")]
public partial class InitialCartorioTccMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Sequências / Tabelas base idempotentes para SQL Server via SqlServerMigrationSql.IgnorandoObjetoExistente
        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Atos] (
    [id] INT NOT NULL IDENTITY(1,1),
    [descricao] VARCHAR(200) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Atos] PRIMARY KEY ([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Cartorios] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nome] VARCHAR(200) NOT NULL,
    [nomeContato] VARCHAR(150) NOT NULL,
    [funcaoContato] VARCHAR(100) NOT NULL,
    [telefoneContato] VARCHAR(20) NOT NULL,
    [logradouro] VARCHAR(200) NOT NULL,
    [numero] VARCHAR(20) NOT NULL,
    [complemento] VARCHAR(100) NULL,
    [bairro] VARCHAR(100) NOT NULL,
    [cidade] VARCHAR(100) NOT NULL,
    [estado] VARCHAR(50) NOT NULL,
    [cep] VARCHAR(10) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Cartorios] PRIMARY KEY ([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Perfis] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nome] VARCHAR(100) NOT NULL,
    [descricao] VARCHAR(250) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Perfis] PRIMARY KEY ([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Paginas] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nome] VARCHAR(120) NOT NULL,
    [rota] VARCHAR(150) NOT NULL,
    [modulo] VARCHAR(80) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Paginas] PRIMARY KEY ([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Paginas_Perfis] (
    [id] INT NOT NULL IDENTITY(1,1),
    [fkPagina] INT NOT NULL,
    [fkPerfil] INT NOT NULL,
    [permitido] BIT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Paginas_Perfis] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Paginas_Perfis_Paginas] FOREIGN KEY ([fkPagina]) REFERENCES [Paginas]([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Paginas_Perfis_Perfis] FOREIGN KEY ([fkPerfil]) REFERENCES [Perfis]([id]) ON DELETE CASCADE
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Funcionarios] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nome] VARCHAR(150) NOT NULL,
    [dataNascimento] DATETIME2 NOT NULL,
    [rg] VARCHAR(20) NOT NULL,
    [orgaoExpedidor] VARCHAR(30) NOT NULL,
    [cpf] VARCHAR(14) NOT NULL,
    [estadoCivil] VARCHAR(40) NOT NULL,
    [email] VARCHAR(150) NOT NULL,
    [telefone] VARCHAR(20) NOT NULL,
    [celular] VARCHAR(20) NULL,
    [logradouro] VARCHAR(200) NOT NULL,
    [numero] VARCHAR(20) NOT NULL,
    [complemento] VARCHAR(100) NULL,
    [bairro] VARCHAR(100) NOT NULL,
    [cidade] VARCHAR(100) NOT NULL,
    [estado] VARCHAR(50) NOT NULL,
    [cep] VARCHAR(10) NOT NULL,
    [dataAdmissao] DATETIME2 NOT NULL,
    [dataDemissao] DATETIME2 NULL,
    [cargo] VARCHAR(80) NOT NULL,
    [login] VARCHAR(120) NOT NULL,
    [senha] VARCHAR(120) NOT NULL,
    [status] INT NOT NULL,
    [fkPerfil] INT NOT NULL,
    [fkCartorio] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Funcionarios] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Funcionarios_Perfis] FOREIGN KEY ([fkPerfil]) REFERENCES [Perfis]([id]),
    CONSTRAINT [FK_Funcionarios_Cartorios] FOREIGN KEY ([fkCartorio]) REFERENCES [Cartorios]([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Tipos_Relatorios] (
    [id] INT NOT NULL IDENTITY(1,1),
    [descricao] VARCHAR(200) NOT NULL,
    [dataGeracao] DATETIME2 NOT NULL,
    [parametros] VARCHAR(500) NULL,
    [fkFuncionario] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Tipos_Relatorios] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Tipos_Relatorios_Funcionarios] FOREIGN KEY ([fkFuncionario]) REFERENCES [Funcionarios]([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Clientes] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nome] VARCHAR(150) NOT NULL,
    [rg] VARCHAR(25) NOT NULL,
    [orgaoExpedidor] VARCHAR(30) NOT NULL,
    [cpfCnpj] VARCHAR(18) NOT NULL,
    [nacionalidade] VARCHAR(60) NOT NULL,
    [dataNascimento] DATETIME2 NOT NULL,
    [profissao] VARCHAR(100) NOT NULL,
    [estadoCivil] VARCHAR(40) NOT NULL,
    [regimeCasamento] VARCHAR(80) NULL,
    [telefone] VARCHAR(20) NOT NULL,
    [celular] VARCHAR(20) NULL,
    [email] VARCHAR(150) NOT NULL,
    [logradouro] VARCHAR(200) NOT NULL,
    [numero] VARCHAR(20) NOT NULL,
    [complemento] VARCHAR(100) NULL,
    [bairro] VARCHAR(100) NOT NULL,
    [cidade] VARCHAR(100) NOT NULL,
    [estado] VARCHAR(50) NOT NULL,
    [cep] VARCHAR(10) NOT NULL,
    [status] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Conjuges] (
    [id] INT NOT NULL IDENTITY(1,1),
    [fkCliente] INT NOT NULL,
    [nome] VARCHAR(150) NOT NULL,
    [rg] VARCHAR(25) NOT NULL,
    [orgaoExpedidor] VARCHAR(30) NOT NULL,
    [cpf] VARCHAR(14) NOT NULL,
    [nacionalidade] VARCHAR(60) NOT NULL,
    [profissao] VARCHAR(100) NOT NULL,
    [telefone] VARCHAR(20) NULL,
    [email] VARCHAR(150) NULL,
    [possuiMesmoEndereco] BIT NOT NULL,
    [logradouro] VARCHAR(200) NOT NULL,
    [numero] VARCHAR(20) NOT NULL,
    [complemento] VARCHAR(100) NULL,
    [bairro] VARCHAR(100) NOT NULL,
    [cidade] VARCHAR(100) NOT NULL,
    [estado] VARCHAR(50) NOT NULL,
    [cep] VARCHAR(10) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Conjuges] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Conjuges_Clientes] FOREIGN KEY ([fkCliente]) REFERENCES [Clientes]([id]) ON DELETE CASCADE
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Certidoes_Casamentos] (
    [id] INT NOT NULL IDENTITY(1,1),
    [fkCliente] INT NOT NULL,
    [cartorioRegistro] VARCHAR(200) NOT NULL,
    [livro] INT NOT NULL,
    [folha] INT NOT NULL,
    [termo] INT NOT NULL,
    [dataCasamento] DATETIME2 NOT NULL,
    [matricula] VARCHAR(80) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Certidoes_Casamentos] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Certidoes_Casamentos_Clientes] FOREIGN KEY ([fkCliente]) REFERENCES [Clientes]([id]) ON DELETE CASCADE
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Certidoes_Outros_Orgaos] (
    [id] INT NOT NULL IDENTITY(1,1),
    [fkCliente] INT NOT NULL,
    [orgaoEmissor] VARCHAR(150) NOT NULL,
    [descricao] VARCHAR(200) NOT NULL,
    [numeroCertidao] VARCHAR(80) NOT NULL,
    [dataEmissao] DATETIME2 NOT NULL,
    [dataValidade] DATETIME2 NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Certidoes_Outros_Orgaos] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Certidoes_Outros_Orgaos_Clientes] FOREIGN KEY ([fkCliente]) REFERENCES [Clientes]([id]) ON DELETE CASCADE
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Imoveis] (
    [id] INT NOT NULL IDENTITY(1,1),
    [tipo] VARCHAR(80) NOT NULL,
    [logradouro] VARCHAR(200) NOT NULL,
    [numero] VARCHAR(20) NOT NULL,
    [complemento] VARCHAR(100) NULL,
    [bairro] VARCHAR(100) NOT NULL,
    [cidade] VARCHAR(100) NOT NULL,
    [estado] VARCHAR(50) NOT NULL,
    [cep] VARCHAR(10) NOT NULL,
    [valorVenal] DECIMAL(18,2) NOT NULL,
    [valor] DECIMAL(18,2) NOT NULL,
    [cartorioRegistro] VARCHAR(200) NOT NULL,
    [numeroMatricula] VARCHAR(60) NOT NULL,
    [cadastroMunicipal] VARCHAR(60) NOT NULL,
    [descricaoCompleta] VARCHAR(2000) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Imoveis] PRIMARY KEY ([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Orcamentos] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nomeCliente] VARCHAR(150) NOT NULL,
    [telefoneContato] VARCHAR(20) NOT NULL,
    [emailContato] VARCHAR(150) NULL,
    [valorBaseTransacao] DECIMAL(18,2) NOT NULL,
    [oneroso] BIT NOT NULL,
    [hipoteca] BIT NOT NULL,
    [ufesp2500] BIT NOT NULL,
    [salarios30] BIT NOT NULL,
    [certidao] BIT NOT NULL,
    [valorCertidao] DECIMAL(18,2) NOT NULL,
    [outros] DECIMAL(18,2) NOT NULL,
    [valorTabelaCustas] DECIMAL(18,2) NOT NULL,
    [descontoAplicado] DECIMAL(18,2) NOT NULL,
    [valorTotal] DECIMAL(18,2) NOT NULL,
    [observacoes] VARCHAR(1000) NULL,
    [fkFuncionario] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Orcamentos] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Orcamentos_Funcionarios] FOREIGN KEY ([fkFuncionario]) REFERENCES [Funcionarios]([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Protocolos] (
    [id] INT NOT NULL IDENTITY(1,1),
    [numero] VARCHAR(10) NOT NULL,
    [solicitante] VARCHAR(150) NOT NULL,
    [cpfSolicitante] VARCHAR(18) NULL,
    [telefoneSolicitante] VARCHAR(20) NOT NULL,
    [emailSolicitante] VARCHAR(150) NULL,
    [dataRecebimento] DATETIME2 NOT NULL,
    [observacoes] VARCHAR(1000) NULL,
    [fkAto] INT NOT NULL,
    [fkFuncionario] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Protocolos] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Protocolos_Atos] FOREIGN KEY ([fkAto]) REFERENCES [Atos]([id]),
    CONSTRAINT [FK_Protocolos_Funcionarios] FOREIGN KEY ([fkFuncionario]) REFERENCES [Funcionarios]([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Escrituras] (
    [id] INT NOT NULL IDENTITY(1,1),
    [livro] INT NOT NULL,
    [folha] INT NOT NULL,
    [dataCriacaoEscritura] DATETIME2 NOT NULL,
    [dataLavratura] DATETIME2 NULL,
    [statusEscritura] INT NOT NULL,
    [statusRegistro] INT NOT NULL,
    [motivoInativacao] VARCHAR(500) NULL,
    [nomeRetirada] VARCHAR(150) NULL,
    [cpfRetirada] VARCHAR(14) NULL,
    [dataRetirada] DATETIME2 NULL,
    [doiExportado] BIT NOT NULL,
    [dataExportacaoDoi] DATETIME2 NULL,
    [textoMinuta] NVARCHAR(MAX) NOT NULL,
    [fkAto] INT NOT NULL,
    [fkProtocolo] INT NOT NULL,
    [fkOrcamento] INT NULL,
    [fkImovel] INT NOT NULL,
    [fkOutorgante] INT NOT NULL,
    [fkOutorgado] INT NOT NULL,
    [fkFuncionario] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Escrituras] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Escrituras_Atos] FOREIGN KEY ([fkAto]) REFERENCES [Atos]([id]),
    CONSTRAINT [FK_Escrituras_Protocolos] FOREIGN KEY ([fkProtocolo]) REFERENCES [Protocolos]([id]),
    CONSTRAINT [FK_Escrituras_Orcamentos] FOREIGN KEY ([fkOrcamento]) REFERENCES [Orcamentos]([id]),
    CONSTRAINT [FK_Escrituras_Imoveis] FOREIGN KEY ([fkImovel]) REFERENCES [Imoveis]([id]),
    CONSTRAINT [FK_Escrituras_Outorgante] FOREIGN KEY ([fkOutorgante]) REFERENCES [Clientes]([id]),
    CONSTRAINT [FK_Escrituras_Outorgado] FOREIGN KEY ([fkOutorgado]) REFERENCES [Clientes]([id]),
    CONSTRAINT [FK_Escrituras_Funcionarios] FOREIGN KEY ([fkFuncionario]) REFERENCES [Funcionarios]([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Documentos_Digitalizados] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nomeArquivo] VARCHAR(255) NOT NULL,
    [contentType] VARCHAR(100) NOT NULL,
    [tamanhoBytes] BIGINT NOT NULL,
    [tipoDocumento] INT NOT NULL,
    [descricao] VARCHAR(250) NOT NULL,
    [conteudoPdf] VARBINARY(MAX) NOT NULL,
    [fkCliente] INT NULL,
    [fkEscritura] INT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Documentos_Digitalizados] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Documentos_Digitalizados_Clientes] FOREIGN KEY ([fkCliente]) REFERENCES [Clientes]([id]),
    CONSTRAINT [FK_Documentos_Digitalizados_Escrituras] FOREIGN KEY ([fkEscritura]) REFERENCES [Escrituras]([id]) ON DELETE CASCADE
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Backups] (
    [id] INT NOT NULL IDENTITY(1,1),
    [nomeArquivo] VARCHAR(255) NOT NULL,
    [caminhoDestino] VARCHAR(500) NOT NULL,
    [periodicidade] VARCHAR(50) NOT NULL,
    [horarioAgendado] VARCHAR(20) NOT NULL,
    [execucaoManual] BIT NOT NULL,
    [criptografado] BIT NOT NULL,
    [algoritmoCriptografia] VARCHAR(50) NOT NULL,
    [hashIntegridade] VARCHAR(128) NOT NULL,
    [tamanhoBytes] BIGINT NOT NULL,
    [dataExecucao] DATETIME2 NOT NULL,
    [sucesso] BIT NOT NULL,
    [mensagemStatus] VARCHAR(500) NOT NULL,
    [fkFuncionario] INT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] VARCHAR(120) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] VARCHAR(120) NULL,
    CONSTRAINT [PK_Backups] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Backups_Funcionarios] FOREIGN KEY ([fkFuncionario]) REFERENCES [Funcionarios]([id])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Historicos_Auditoria] (
    [id] INT NOT NULL IDENTITY(1,1),
    [entidade] VARCHAR(120) NOT NULL,
    [chavePrimaria] VARCHAR(80) NOT NULL,
    [acao] INT NOT NULL,
    [usuarioLogin] VARCHAR(120) NOT NULL,
    [dataOcorrencia] DATETIME2 NOT NULL,
    [valoresDetalhe] VARCHAR(4000) NOT NULL,
    CONSTRAINT [PK_Historicos_Auditoria] PRIMARY KEY ([id])
);"));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Historicos_Auditoria];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Backups];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Documentos_Digitalizados];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Escrituras];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Protocolos];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Orcamentos];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Imoveis];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Certidoes_Outros_Orgaos];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Certidoes_Casamentos];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Conjuges];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Clientes];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Tipos_Relatorios];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Funcionarios];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Paginas_Perfis];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Paginas];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Perfis];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Cartorios];");
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Atos];");
    }
}
