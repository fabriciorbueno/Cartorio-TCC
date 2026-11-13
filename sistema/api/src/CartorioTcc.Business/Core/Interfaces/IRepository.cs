using CartorioTcc.Business.Core.Entities;

namespace CartorioTcc.Business.Core.Interfaces;

public interface IUnitOfWork
{
    /// <summary>Persiste de forma atômica todas as alterações rastreadas.</summary>
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repositório do lado de escrita (Commands). Leituras otimizadas ficam nos QueryServices.
/// </summary>
public interface IRepository<TEntity> : IDisposable where TEntity : Entity
{
    IUnitOfWork UnitOfWork { get; }
    Task<TEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    void Adicionar(TEntity entity);
    void Atualizar(TEntity entity);
    void Remover(TEntity entity);
}

/// <summary>Contexto do usuário da requisição (autenticação temporariamente inativa = usuário mock).</summary>
public interface IUserContext
{
    string Login { get; }
    string Nome { get; }
    int? FuncionarioId { get; }
    bool Autenticado { get; }
    IReadOnlyCollection<string> Perfis { get; }
}

/// <summary>Abstração do relógio para facilitar testes.</summary>
public interface IDateTimeProvider
{
    DateTime Agora { get; }
    DateOnly Hoje { get; }
}

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime Agora => DateTime.Now;
    public DateOnly Hoje => DateOnly.FromDateTime(DateTime.Now);
}
