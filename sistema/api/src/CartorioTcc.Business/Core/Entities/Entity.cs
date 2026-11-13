namespace CartorioTcc.Business.Core.Entities;

/// <summary>
/// Entidade base do domínio (identidade numérica gerada pelo banco - IDENTITY).
/// </summary>
public abstract class Entity
{
    public int Id { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id != 0 && Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
