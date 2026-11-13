using System.Text.Json;
using CartorioTcc.Business.Core.Entities;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Enums;
using Microsoft.EntityFrameworkCore;

namespace CartorioTcc.Infra.Data.Context;

/// <summary>
/// DbContext principal do sistema CartorioTcc.
/// Implementa <c>IUnitOfWork</c>, preenche automaticamente <c>DT_CRIACAO</c>, <c>LOGIN_USUARIO_CRIACAO</c>,
/// <c>DT_ALTERACAO</c> e <c>LOGIN_USUARIO_ALTERACAO</c> em todas as <c>AuditableEntity</c> e grava
/// o histórico de auditoria (<c>AcaoAuditoria</c>: Inclusão, Alteração, Exclusão).
/// </summary>
public sealed class CartorioTccContext(
    DbContextOptions<CartorioTccContext> options,
    IUserContext? userContext = null,
    IDateTimeProvider? dateTimeProvider = null) : DbContext(options), IUnitOfWork
{
    public DbSet<Ato> Atos => Set<Ato>();
    public DbSet<Cartorio> Cartorios => Set<Cartorio>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Pagina> Paginas => Set<Pagina>();
    public DbSet<PaginaPerfil> PaginasPerfis => Set<PaginaPerfil>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Conjuge> Conjuges => Set<Conjuge>();
    public DbSet<CertidaoCasamento> CertidoesCasamentos => Set<CertidaoCasamento>();
    public DbSet<CertidaoOutroOrgao> CertidoesOutrosOrgaos => Set<CertidaoOutroOrgao>();
    public DbSet<Imovel> Imoveis => Set<Imovel>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<Protocolo> Protocolos => Set<Protocolo>();
    public DbSet<Escritura> Escrituras => Set<Escritura>();
    public DbSet<DocumentoDigitalizado> DocumentosDigitalizados => Set<DocumentoDigitalizado>();
    public DbSet<BackupRegistro> Backups => Set<BackupRegistro>();
    public DbSet<TipoRelatorio> TiposRelatorios => Set<TipoRelatorio>();
    public DbSet<HistoricoAuditoria> HistoricoAuditorias => Set<HistoricoAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartorioTccContext).Assembly);
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        AplicarAuditoriaAutomatica();
        return await SaveChangesAsync(cancellationToken) > 0;
    }

    private void AplicarAuditoriaAutomatica()
    {
        var login = string.IsNullOrWhiteSpace(userContext?.Login) ? "admin@saec.com" : userContext!.Login;
        var agora = dateTimeProvider?.Agora ?? DateTime.Now;
        var registrosHistorico = new List<HistoricoAuditoria>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>().ToList())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var tabela = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
            var acao = entry.State switch
            {
                EntityState.Added => AcaoAuditoria.Inclusao,
                EntityState.Modified => AcaoAuditoria.Alteracao,
                _ => AcaoAuditoria.Exclusao
            };

            if (entry.State == EntityState.Added)
            {
                entry.Entity.RegistrarCriacao(login, agora);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.DtCriacao).IsModified = false;
                entry.Property(x => x.LoginUsuarioCriacao).IsModified = false;
                entry.Entity.RegistrarAlteracao(login, agora);
            }

            string? antigoJson = null;
            string? novoJson = null;

            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                var orig = entry.Properties
                    .Where(p => p.Metadata.ClrType != typeof(byte[]))
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                antigoJson = JsonSerializer.Serialize(orig);
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var curr = entry.Properties
                    .Where(p => p.Metadata.ClrType != typeof(byte[]))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                novoJson = JsonSerializer.Serialize(curr);
            }

            registrosHistorico.Add(new HistoricoAuditoria(
                tabela,
                entry.Entity.Id.ToString(),
                acao,
                login,
                agora,
                antigoJson,
                novoJson));
        }

        if (registrosHistorico.Count > 0)
            HistoricoAuditorias.AddRange(registrosHistorico);
    }
}
