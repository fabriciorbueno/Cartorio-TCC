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
        migrationBuilder.Sql(SqlServerMigrationSql.ScriptCriacaoInicialIdempotente());

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Atos] (
    [Id_Ato] INT NOT NULL IDENTITY(1,1),
    [Tipo_Ato] NVARCHAR(80) NOT NULL,
    [Descricao_Ato] NVARCHAR(2000) NOT NULL,
    [Modelo_Minuta_Padrao] NVARCHAR(4000) NULL,
    [Exige_Imovel] BIT NOT NULL,
    [Gera_Doi] BIT NOT NULL,
    [Ativo] BIT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Atos] PRIMARY KEY ([Id_Ato])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Cartorios] (
    [Id_Cartorio] INT NOT NULL IDENTITY(1,1),
    [Nome_Cartorio] NVARCHAR(120) NOT NULL,
    [CNPJ_Cartorio] NVARCHAR(18) NOT NULL,
    [Tabeliao_Responsavel] NVARCHAR(120) NULL,
    [Endereco_Completo] NVARCHAR(250) NULL,
    [Telefone] NVARCHAR(25) NULL,
    [Email] NVARCHAR(100) NULL,
    [Codigo_CNS] NVARCHAR(20) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Cartorios] PRIMARY KEY ([Id_Cartorio])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Perfis] (
    [Id_Perfil] INT NOT NULL IDENTITY(1,1),
    [Nome_Perfil] NVARCHAR(60) NOT NULL,
    [Descricao_Perfil] NVARCHAR(250) NULL,
    [Id_Status_Perfil] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Perfis] PRIMARY KEY ([Id_Perfil])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Paginas] (
    [Id_Pagina] INT NOT NULL IDENTITY(1,1),
    [Nome_Pagina] NVARCHAR(80) NOT NULL,
    [URL] NVARCHAR(150) NOT NULL,
    [Modulo] NVARCHAR(60) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Paginas] PRIMARY KEY ([Id_Pagina])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Paginas_Perfis] (
    [Id_Pagina_Perfil] INT NOT NULL IDENTITY(1,1),
    [Id_Perfil] INT NOT NULL,
    [Id_Pagina] INT NOT NULL,
    [Pode_Visualizar] BIT NOT NULL,
    [Pode_Incluir] BIT NOT NULL,
    [Pode_Editar] BIT NOT NULL,
    [Pode_Excluir] BIT NOT NULL,
    [Id_Status_Perfil] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Paginas_Perfis] PRIMARY KEY ([Id_Pagina_Perfil]),
    CONSTRAINT [FK_Paginas_Perfis_Perfis_Id_Perfil] FOREIGN KEY ([Id_Perfil]) REFERENCES [Perfis]([Id_Perfil]),
    CONSTRAINT [FK_Paginas_Perfis_Paginas_Id_Pagina] FOREIGN KEY ([Id_Pagina]) REFERENCES [Paginas]([Id_Pagina])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE UNIQUE INDEX [IX_Paginas_Perfis_Id_Perfil_Id_Pagina] ON [Paginas_Perfis]([Id_Perfil], [Id_Pagina]);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Funcionarios] (
    [Id_Funcionario] INT NOT NULL IDENTITY(1,1),
    [Id_Perfil] INT NOT NULL,
    [Nome_Funcionario] NVARCHAR(100) NOT NULL,
    [Data_Nasc_Funcionario] DATE NOT NULL,
    [Data_Admissao_Funcionario] DATE NULL,
    [Email_Funcionario] NVARCHAR(100) NOT NULL,
    [Senha_Funcionario] NVARCHAR(128) NOT NULL,
    [CPF_Funcionario] NVARCHAR(14) NOT NULL,
    [RG_Funcionario] NVARCHAR(30) NOT NULL,
    [Telefone_Funcionario] NVARCHAR(20) NULL,
    [Cargo_Funcionario] NVARCHAR(60) NULL,
    [Id_Status_Funcionario] INT NOT NULL,
    [CEP_Funcionario] NVARCHAR(8) NOT NULL,
    [Pais_Funcionario] NVARCHAR(40) NOT NULL,
    [Estado_Funcionario] NVARCHAR(2) NOT NULL,
    [Cidade_Funcionario] NVARCHAR(60) NOT NULL,
    [Bairro_Funcionario] NVARCHAR(60) NOT NULL,
    [Logradouro_Funcionario] NVARCHAR(120) NOT NULL,
    [Numero_Funcionario] NVARCHAR(20) NOT NULL,
    [Complemento_Funcionario] NVARCHAR(60) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Funcionarios] PRIMARY KEY ([Id_Funcionario]),
    CONSTRAINT [FK_Funcionarios_Perfis_Id_Perfil] FOREIGN KEY ([Id_Perfil]) REFERENCES [Perfis]([Id_Perfil])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE UNIQUE INDEX [IX_Funcionarios_CPF_Funcionario] ON [Funcionarios]([CPF_Funcionario]);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Tipos_Relatorios] (
    [Id_Relatorio] INT NOT NULL IDENTITY(1,1),
    [Codigo_Relatorio] INT NOT NULL,
    [Tipo_Relatorios] NVARCHAR(80) NOT NULL,
    [Descricao_Relatorio] NVARCHAR(1000) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Tipos_Relatorios] PRIMARY KEY ([Id_Relatorio])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Clientes] (
    [Id_Cliente] INT NOT NULL IDENTITY(1,1),
    [Nome_Cliente] NVARCHAR(100) NOT NULL,
    [RG_Clientes] NVARCHAR(30) NOT NULL,
    [Orgao_Expedidor_Cliente] NVARCHAR(20) NOT NULL,
    [Nacionalidade_Cliente] NVARCHAR(40) NOT NULL,
    [Estado_Orgao_Expedidor_Cliente] NVARCHAR(2) NOT NULL,
    [CPF_Cliente] NVARCHAR(14) NOT NULL,
    [Data_Nasc_Cliente] DATE NOT NULL,
    [Email_Cliente] NVARCHAR(100) NOT NULL,
    [Genero_Cliente] NVARCHAR(1) NOT NULL,
    [Estado_Civil_Cliente] NVARCHAR(30) NOT NULL,
    [Regime_Bens_Cliente] NVARCHAR(60) NULL,
    [Profissao_Cliente] NVARCHAR(60) NOT NULL,
    [Telefone_Cliente] NVARCHAR(20) NOT NULL,
    [Id_Status_Cliente] INT NOT NULL,
    [CEP_Cliente] NVARCHAR(8) NOT NULL,
    [Pais_Cliente] NVARCHAR(40) NOT NULL,
    [Estado_Cliente] NVARCHAR(2) NOT NULL,
    [Cidade_Cliente] NVARCHAR(60) NOT NULL,
    [Bairro_Cliente] NVARCHAR(60) NOT NULL,
    [Logradouro_Cliente] NVARCHAR(120) NOT NULL,
    [Numero_Cliente] NVARCHAR(20) NOT NULL,
    [Complemento_Cliente] NVARCHAR(60) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id_Cliente])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE UNIQUE INDEX [IX_Clientes_CPF_Cliente] ON [Clientes]([CPF_Cliente]);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Conjuges] (
    [Id_Conjuge] INT NOT NULL IDENTITY(1,1),
    [Id_Cliente] INT NOT NULL,
    [Nome_Conjuge] NVARCHAR(100) NOT NULL,
    [RG_Conjuge] NVARCHAR(30) NOT NULL,
    [Orgao_Expedidor_Conjuge] NVARCHAR(20) NOT NULL,
    [Nacionalidade_Conjuge] NVARCHAR(40) NOT NULL,
    [Estado_Orgao_Expedidor_Conjuge] NVARCHAR(2) NOT NULL,
    [CPF_Conjuge] NVARCHAR(14) NOT NULL,
    [Data_Nasc_Conjuge] DATE NOT NULL,
    [Email_Conjuge] NVARCHAR(100) NULL,
    [Genero_Conjuge] NVARCHAR(1) NULL,
    [Estado_Civil_Conjuge] NVARCHAR(30) NULL,
    [Profissao_Conjuge] NVARCHAR(60) NULL,
    [Telefone_Conjuge] NVARCHAR(20) NULL,
    [Id_Status_Cliente] INT NOT NULL,
    [CEP_Conjuge] NVARCHAR(8) NOT NULL,
    [Pais_Conjuge] NVARCHAR(40) NOT NULL,
    [Estado_Conjuge] NVARCHAR(2) NOT NULL,
    [Cidade_Conjuge] NVARCHAR(60) NOT NULL,
    [Bairro_Conjuge] NVARCHAR(60) NOT NULL,
    [Logradouro_Conjuge] NVARCHAR(120) NOT NULL,
    [Numero_Conjuge] NVARCHAR(20) NOT NULL,
    [Complemento_Conjuge] NVARCHAR(60) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Conjuges] PRIMARY KEY ([Id_Conjuge]),
    CONSTRAINT [FK_Conjuges_Clientes_Id_Cliente] FOREIGN KEY ([Id_Cliente]) REFERENCES [Clientes]([Id_Cliente]) ON DELETE CASCADE
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE UNIQUE INDEX [IX_Conjuges_Id_Cliente] ON [Conjuges]([Id_Cliente]);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Certidoes_Casamentos] (
    [Id_Certidao_Casamento] INT NOT NULL IDENTITY(1,1),
    [Id_Cliente] INT NOT NULL,
    [Id_Conjuge] INT NULL,
    [Cartorio_Certidao_Casamento] NVARCHAR(120) NOT NULL,
    [Livro_Certidao_Casamento] NVARCHAR(50) NOT NULL,
    [Folhas_Certidao_Casamento] NVARCHAR(50) NOT NULL,
    [Termo_Certidao_Casamento] NVARCHAR(50) NOT NULL,
    [Data_Certidao_Casamento] DATE NOT NULL,
    [Averbacoes] NVARCHAR(500) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Certidoes_Casamentos] PRIMARY KEY ([Id_Certidao_Casamento]),
    CONSTRAINT [FK_Certidoes_Casamentos_Clientes_Id_Cliente] FOREIGN KEY ([Id_Cliente]) REFERENCES [Clientes]([Id_Cliente]) ON DELETE CASCADE,
    CONSTRAINT [FK_Certidoes_Casamentos_Conjuges_Id_Conjuge] FOREIGN KEY ([Id_Conjuge]) REFERENCES [Conjuges]([Id_Conjuge])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE UNIQUE INDEX [IX_Certidoes_Casamentos_Id_Cliente] ON [Certidoes_Casamentos]([Id_Cliente]);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Certidoes_Outros_Orgaos] (
    [Id_Certidao_Outro_Orgao] INT NOT NULL IDENTITY(1,1),
    [Cartorio_Certidao_Outro_Orgao] NVARCHAR(120) NOT NULL,
    [Livro_Certidao_Outro_Orgao] NVARCHAR(50) NOT NULL,
    [Folhas_Certidao_Outro_Orgao] NVARCHAR(50) NOT NULL,
    [Termo_Certidao_Outro_Orgao] NVARCHAR(50) NOT NULL,
    [Data_Certidao_Outro_Orgao] DATE NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Certidoes_Outros_Orgaos] PRIMARY KEY ([Id_Certidao_Outro_Orgao])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Imoveis] (
    [Id_Imovel] INT NOT NULL IDENTITY(1,1),
    [Id_Cliente] INT NULL,
    [Numero_Registro] NVARCHAR(40) NOT NULL,
    [Valor_Venal] DECIMAL(17,2) NOT NULL,
    [Nome_Prefeitura] NVARCHAR(100) NOT NULL,
    [Descricao_Imovel] NVARCHAR(2000) NULL,
    [Valor_Imovel] DECIMAL(17,2) NOT NULL,
    [Area_M2] DECIMAL(14,2) NULL,
    [Valor_Por_M2] DECIMAL(17,2) NULL,
    [Inscricao_Municipal] NVARCHAR(50) NULL,
    [Cartorio_Registro_Imoveis] NVARCHAR(120) NULL,
    [CEP_Imovel] NVARCHAR(8) NOT NULL,
    [Pais_Imovel] NVARCHAR(40) NOT NULL,
    [Estado_Imovel] NVARCHAR(2) NOT NULL,
    [Cidade_Imovel] NVARCHAR(60) NOT NULL,
    [Bairro_Imovel] NVARCHAR(60) NOT NULL,
    [Logradouro_Imovel] NVARCHAR(120) NOT NULL,
    [Numero_Imovel] NVARCHAR(20) NOT NULL,
    [Complemento_Imovel] NVARCHAR(60) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Imoveis] PRIMARY KEY ([Id_Imovel]),
    CONSTRAINT [FK_Imoveis_Clientes_Id_Cliente] FOREIGN KEY ([Id_Cliente]) REFERENCES [Clientes]([Id_Cliente])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Orcamentos] (
    [Id_Orcamento] INT NOT NULL IDENTITY(1,1),
    [Id_Cliente] INT NULL,
    [Valor_Orcamento] DECIMAL(17,2) NOT NULL,
    [Oneroso_Orcamento] BIT NOT NULL,
    [Hipoteca_Orcamento] BIT NOT NULL,
    [UFESP2500_Orcamento] BIT NOT NULL,
    [Salarios30_Orcamento] BIT NOT NULL,
    [Certidao_Orcamento] BIT NOT NULL,
    [Outros_Orcamento] DECIMAL(17,2) NOT NULL,
    [Faixa_Descricao] NVARCHAR(120) NOT NULL,
    [Emolumentos_Tabelionato] DECIMAL(17,2) NOT NULL,
    [Emolumentos_Estado] DECIMAL(17,2) NOT NULL,
    [Contribuicao_IPESP] DECIMAL(17,2) NOT NULL,
    [Fundo_Registro_Civil] DECIMAL(17,2) NOT NULL,
    [Taxa_Tribunal_Justica] DECIMAL(17,2) NOT NULL,
    [Fundo_Ministerio_Publico] DECIMAL(17,2) NOT NULL,
    [ISSQN_Estimado] DECIMAL(17,2) NOT NULL,
    [Valor_Certidao] DECIMAL(17,2) NOT NULL,
    [Previsao_Registro_Imoveis] DECIMAL(17,2) NOT NULL,
    [Valor_Total_Orcamento] DECIMAL(17,2) NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Orcamentos] PRIMARY KEY ([Id_Orcamento]),
    CONSTRAINT [FK_Orcamentos_Clientes_Id_Cliente] FOREIGN KEY ([Id_Cliente]) REFERENCES [Clientes]([Id_Cliente])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Protocolos] (
    [Id_Protocolo] INT NOT NULL IDENTITY(1,1),
    [Numero_Protocolo] NVARCHAR(20) NOT NULL,
    [Id_Cliente] INT NOT NULL,
    [Id_Ato] INT NULL,
    [Id_Funcionario] INT NULL,
    [Id_Orcamento] INT NULL,
    [Natureza_Pedido] NVARCHAR(150) NULL,
    [Documentos_Apresentados] NVARCHAR(1000) NULL,
    [Documentos_Faltantes] NVARCHAR(1000) NULL,
    [Valor_Orcamento_Estimado] DECIMAL(17,2) NULL,
    [Observacoes] NVARCHAR(1000) NULL,
    [Id_Status_Escritura] INT NOT NULL,
    [Id_Status_Protocolo] INT NOT NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Protocolos] PRIMARY KEY ([Id_Protocolo]),
    CONSTRAINT [FK_Protocolos_Clientes_Id_Cliente] FOREIGN KEY ([Id_Cliente]) REFERENCES [Clientes]([Id_Cliente]),
    CONSTRAINT [FK_Protocolos_Atos_Id_Ato] FOREIGN KEY ([Id_Ato]) REFERENCES [Atos]([Id_Ato]),
    CONSTRAINT [FK_Protocolos_Funcionarios_Id_Funcionario] FOREIGN KEY ([Id_Funcionario]) REFERENCES [Funcionarios]([Id_Funcionario]),
    CONSTRAINT [FK_Protocolos_Orcamentos_Id_Orcamento] FOREIGN KEY ([Id_Orcamento]) REFERENCES [Orcamentos]([Id_Orcamento])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE UNIQUE INDEX [IX_Protocolos_Numero_Protocolo] ON [Protocolos]([Numero_Protocolo]);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Escrituras] (
    [Id_Escritura] INT NOT NULL IDENTITY(1,1),
    [Id_Cartorio] INT NOT NULL,
    [Id_Outorgante] INT NOT NULL,
    [Id_Outorgado] INT NOT NULL,
    [Id_Funcionario] INT NOT NULL,
    [Id_Protocolo] INT NOT NULL,
    [Id_Ato] INT NOT NULL,
    [Id_Imovel] INT NULL,
    [Id_Certidao_Outro_Orgao] INT NULL,
    [Numero_Escritura] NVARCHAR(30) NOT NULL,
    [Numero_Livro] INT NOT NULL,
    [Folha_Inicial] INT NOT NULL,
    [Folha_Final] INT NOT NULL,
    [Valor_Operacao] DECIMAL(17,2) NOT NULL,
    [Texto_Minuta] NVARCHAR(MAX) NULL,
    [Id_Status_Escritura] INT NOT NULL,
    [Motivo_Inativacao] NVARCHAR(500) NULL,
    [Data_Inicio_Processo] DATETIME2 NOT NULL,
    [Data_Lavratura] DATETIME2 NULL,
    [Data_Fim_Processo] DATETIME2 NULL,
    [Data_Envio_DOI] DATETIME2 NULL,
    [Documentos_Retirados] BIT NOT NULL,
    [Data_Retirada_Documentos] DATETIME2 NULL,
    [Observacoes_Retirada] NVARCHAR(500) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Escrituras] PRIMARY KEY ([Id_Escritura]),
    CONSTRAINT [FK_Escrituras_Cartorios_Id_Cartorio] FOREIGN KEY ([Id_Cartorio]) REFERENCES [Cartorios]([Id_Cartorio]),
    CONSTRAINT [FK_Escrituras_Clientes_Id_Outorgante] FOREIGN KEY ([Id_Outorgante]) REFERENCES [Clientes]([Id_Cliente]),
    CONSTRAINT [FK_Escrituras_Clientes_Id_Outorgado] FOREIGN KEY ([Id_Outorgado]) REFERENCES [Clientes]([Id_Cliente]),
    CONSTRAINT [FK_Escrituras_Funcionarios_Id_Funcionario] FOREIGN KEY ([Id_Funcionario]) REFERENCES [Funcionarios]([Id_Funcionario]),
    CONSTRAINT [FK_Escrituras_Protocolos_Id_Protocolo] FOREIGN KEY ([Id_Protocolo]) REFERENCES [Protocolos]([Id_Protocolo]),
    CONSTRAINT [FK_Escrituras_Atos_Id_Ato] FOREIGN KEY ([Id_Ato]) REFERENCES [Atos]([Id_Ato]),
    CONSTRAINT [FK_Escrituras_Imoveis_Id_Imovel] FOREIGN KEY ([Id_Imovel]) REFERENCES [Imoveis]([Id_Imovel]),
    CONSTRAINT [FK_Escrituras_Certidoes_Outros_Orgaos_Id_Certidao_Outro_Orgao] FOREIGN KEY ([Id_Certidao_Outro_Orgao]) REFERENCES [Certidoes_Outros_Orgaos]([Id_Certidao_Outro_Orgao])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Documentos_Digitalizados] (
    [Id_Documento] INT NOT NULL IDENTITY(1,1),
    [Id_Escritura] INT NULL,
    [Id_Cliente] INT NULL,
    [Tipo_Documento] INT NOT NULL,
    [Nome_Arquivo] NVARCHAR(180) NOT NULL,
    [Content_Type] NVARCHAR(80) NOT NULL,
    [Tamanho_Bytes] BIGINT NOT NULL,
    [Hash_SHA256] NVARCHAR(64) NOT NULL,
    [Conteudo_PDF] VARBINARY(MAX) NOT NULL,
    [Observacao] NVARCHAR(300) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Documentos_Digitalizados] PRIMARY KEY ([Id_Documento]),
    CONSTRAINT [FK_Documentos_Digitalizados_Escrituras_Id_Escritura] FOREIGN KEY ([Id_Escritura]) REFERENCES [Escrituras]([Id_Escritura]) ON DELETE CASCADE,
    CONSTRAINT [FK_Documentos_Digitalizados_Clientes_Id_Cliente] FOREIGN KEY ([Id_Cliente]) REFERENCES [Clientes]([Id_Cliente])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Backups] (
    [Id_Backup] INT NOT NULL IDENTITY(1,1),
    [Data_Backup] DATETIME2 NOT NULL,
    [Data_Agendada] DATETIME2 NULL,
    [Descricao_Backup] NVARCHAR(2000) NOT NULL,
    [Id_Funcionario] INT NULL,
    [Manual] BIT NOT NULL,
    [Criptografado] BIT NOT NULL,
    [Executado] BIT NOT NULL,
    [Nome_Arquivo_Gerado] NVARCHAR(150) NOT NULL,
    [Tamanho_Bytes] BIGINT NOT NULL,
    [Hash_Integridade] NVARCHAR(64) NOT NULL,
    [Conteudo_Criptografado] VARBINARY(MAX) NULL,
    [DT_CRIACAO] DATETIME2 NOT NULL,
    [LOGIN_USUARIO_CRIACAO] NVARCHAR(80) NOT NULL,
    [DT_ALTERACAO] DATETIME2 NULL,
    [LOGIN_USUARIO_ALTERACAO] NVARCHAR(80) NULL,
    CONSTRAINT [PK_Backups] PRIMARY KEY ([Id_Backup]),
    CONSTRAINT [FK_Backups_Funcionarios_Id_Funcionario] FOREIGN KEY ([Id_Funcionario]) REFERENCES [Funcionarios]([Id_Funcionario])
);"));

        migrationBuilder.Sql(SqlServerMigrationSql.IgnorandoObjetoExistente(@"
CREATE TABLE [Historico_Auditoria] (
    [Id_Auditoria] INT NOT NULL IDENTITY(1,1),
    [Tabela] NVARCHAR(100) NOT NULL,
    [Chave_Primaria] NVARCHAR(60) NOT NULL,
    [Acao_Auditoria] INT NOT NULL,
    [Login_Usuario] NVARCHAR(80) NOT NULL,
    [Data_Ocorrencia] DATETIME2 NOT NULL,
    [Valores_Antigos_Json] NVARCHAR(MAX) NULL,
    [Valores_Novos_Json] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_Historico_Auditoria] PRIMARY KEY ([Id_Auditoria])
);"));

        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
    WHERE c.object_id = OBJECT_ID(N'dbo.Historico_Auditoria')
      AND c.name = N'Id_Auditoria'
      AND t.name = N'bigint'
)
BEGIN
    DROP TABLE [dbo].[Historico_Auditoria];
    CREATE TABLE [dbo].[Historico_Auditoria] (
        [Id_Auditoria] INT NOT NULL IDENTITY(1,1),
        [Tabela] NVARCHAR(100) NOT NULL,
        [Chave_Primaria] NVARCHAR(60) NOT NULL,
        [Acao_Auditoria] INT NOT NULL,
        [Login_Usuario] NVARCHAR(80) NOT NULL,
        [Data_Ocorrencia] DATETIME2 NOT NULL,
        [Valores_Antigos_Json] NVARCHAR(MAX) NULL,
        [Valores_Novos_Json] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Historico_Auditoria] PRIMARY KEY ([Id_Auditoria])
    );
END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS [Historico_Auditoria];");
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
