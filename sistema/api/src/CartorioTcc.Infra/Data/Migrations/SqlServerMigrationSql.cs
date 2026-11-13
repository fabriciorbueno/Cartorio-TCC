namespace CartorioTcc.Infra.Data.Migrations;

/// <summary>
/// Helpers de resiliência para Migrations SQL Server com raw SQL idempotente
/// (<c>SqlServerMigrationSql.IgnorandoObjetoExistente(...)</c>), assegurando idempotência na execução
/// dos scripts de DDL no SQL Server (requisito 2.E da arquitetura).
/// </summary>
public static class SqlServerMigrationSql
{
    private const int ErroObjetoJaExiste = 2714;
    private const int ErroColunaJaExiste = 2705;
    private const int ErroIndiceJaExiste = 1913;
    private const int ErroConstraintJaExiste = 1781;

    /// <summary>
    /// Envolve um comando DDL SQL Server em bloco <c>BEGIN TRY ... END TRY BEGIN CATCH</c>
    /// ignorando erros de objeto/coluna/índice já existente para garantir idempotência.
    /// </summary>
    public static string IgnorandoObjetoExistente(string rawSql) =>
        $"""
        BEGIN TRY
            EXEC sp_executesql N'{rawSql.Replace("'", "''")}';
        END TRY
        BEGIN CATCH
            IF ERROR_NUMBER() NOT IN ({ErroObjetoJaExiste}, {ErroColunaJaExiste}, {ErroIndiceJaExiste}, {ErroConstraintJaExiste})
                THROW;
        END CATCH
        """;

    public static string ScriptCriacaoInicialIdempotente() =>
        IgnorandoObjetoExistente(
            """
            IF OBJECT_ID(N'dbo.Sequencia_Protocolo', N'SO') IS NULL
                CREATE SEQUENCE dbo.Sequencia_Protocolo AS INT START WITH 10 INCREMENT BY 1;

            IF OBJECT_ID(N'dbo.Sequencia_Auditoria', N'SO') IS NULL
                CREATE SEQUENCE dbo.Sequencia_Auditoria AS INT START WITH 1 INCREMENT BY 1;
            """);
}
