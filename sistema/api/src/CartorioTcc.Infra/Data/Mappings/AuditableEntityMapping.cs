using CartorioTcc.Business.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartorioTcc.Infra.Data.Mappings;

/// <summary>
/// Mapeamento Fluent API padronizado para todas as entidades auditáveis (<c>AuditableEntity</c>),
/// mapeando <c>DT_CRIACAO</c>, <c>LOGIN_USUARIO_CRIACAO</c>, <c>DT_ALTERACAO</c> e <c>LOGIN_USUARIO_ALTERACAO</c>.
/// </summary>
public abstract class AuditableEntityMapping<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DtCriacao)
            .HasColumnName("DT_CRIACAO")
            .IsRequired();

        builder.Property(e => e.LoginUsuarioCriacao)
            .HasColumnName("LOGIN_USUARIO_CRIACAO")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(e => e.DtAlteracao)
            .HasColumnName("DT_ALTERACAO");

        builder.Property(e => e.LoginUsuarioAlteracao)
            .HasColumnName("LOGIN_USUARIO_ALTERACAO")
            .HasMaxLength(80);
    }
}
