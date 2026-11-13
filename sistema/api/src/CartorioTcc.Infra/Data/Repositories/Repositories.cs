using CartorioTcc.Business.Core.Entities;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CartorioTcc.Infra.Data.Repositories;

/// <summary>
/// Repositório genérico para o lado de escrita (Commands / Unit of Work).
/// </summary>
public abstract class BaseRepository<TEntity>(CartorioTccContext context) : IRepository<TEntity>
    where TEntity : Entity
{
    protected CartorioTccContext Context { get; } = context;
    protected DbSet<TEntity> DbSet { get; } = context.Set<TEntity>();

    public IUnitOfWork UnitOfWork => Context;

    public virtual async Task<TEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual void Adicionar(TEntity entity) => DbSet.Add(entity);

    public virtual void Atualizar(TEntity entity) => DbSet.Update(entity);

    public virtual void Remover(TEntity entity) => DbSet.Remove(entity);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public sealed class PermissaoRepository(CartorioTccContext context)
    : BaseRepository<PaginaPerfil>(context), IPermissaoRepository
{
    public async Task<bool> ExistePermissaoAsync(int idPerfil, int idPagina, int? ignorarId = null, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().AnyAsync(
            p => p.IdPerfil == idPerfil && p.IdPagina == idPagina && (ignorarId == null || p.Id != ignorarId.Value),
            ct);
}

public sealed class FuncionarioRepository(CartorioTccContext context)
    : BaseRepository<Funcionario>(context), IFuncionarioRepository
{
    public async Task<bool> ExisteCpfOuEmailAsync(string cpf, string email, int? ignorarId = null, CancellationToken ct = default)
    {
        var emailLower = email.Trim().ToLowerInvariant();
        return await DbSet.AsNoTracking().AnyAsync(
            f => (f.CpfFuncionario == cpf || f.EmailFuncionario == emailLower) &&
                 (ignorarId == null || f.Id != ignorarId.Value),
            ct);
    }
}

public sealed class ClienteRepository(CartorioTccContext context)
    : BaseRepository<Cliente>(context), IClienteRepository
{
    public async Task<Cliente?> ObterCompletoPorIdAsync(int id, CancellationToken ct = default) =>
        await DbSet
            .Include(c => c.Conjuge)
            .Include(c => c.CertidaoCasamento)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken ct = default) =>
        await DbSet
            .Include(c => c.Conjuge)
            .Include(c => c.CertidaoCasamento)
            .FirstOrDefaultAsync(c => c.CpfCliente == cpf, ct);

    public async Task<bool> ExisteCpfAsync(string cpf, int? ignorarId = null, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().AnyAsync(
            c => c.CpfCliente == cpf && (ignorarId == null || c.Id != ignorarId.Value),
            ct);
}

public sealed class OrcamentoRepository(CartorioTccContext context)
    : BaseRepository<Orcamento>(context), IOrcamentoRepository;

public sealed class ProtocoloRepository(CartorioTccContext context)
    : BaseRepository<Protocolo>(context), IProtocoloRepository
{
    public async Task<Protocolo?> ObterPorNumeroAsync(string numeroProtocolo, CancellationToken ct = default)
    {
        var norm = numeroProtocolo.Trim().ToUpperInvariant();
        return await DbSet.FirstOrDefaultAsync(
            p => p.NumeroProtocolo == norm || p.Id.ToString() == norm,
            ct);
    }

    public async Task<int> ObterProximoSequencialAsync(int ano, CancellationToken ct = default)
    {
        var prefixo = ano.ToString("D4");
        var totalNoAno = await DbSet.AsNoTracking()
            .CountAsync(p => p.NumeroProtocolo.StartsWith(prefixo), ct);
        return totalNoAno + 1;
    }
}

public sealed class EscrituraRepository(CartorioTccContext context)
    : BaseRepository<Escritura>(context), IEscrituraRepository
{
    public async Task<Escritura?> ObterCompletaPorIdAsync(int id, CancellationToken ct = default) =>
        await DbSet
            .Include(e => e.Cartorio)
            .Include(e => e.Ato)
            .Include(e => e.Protocolo)
            .Include(e => e.Funcionario)
            .Include(e => e.Outorgante).ThenInclude(c => c!.Conjuge)
            .Include(e => e.Outorgante).ThenInclude(c => c!.CertidaoCasamento)
            .Include(e => e.Outorgado).ThenInclude(c => c!.Conjuge)
            .Include(e => e.Outorgado).ThenInclude(c => c!.CertidaoCasamento)
            .Include(e => e.Imovel)
            .Include(e => e.CertidaoOutroOrgao)
            .Include(e => e.Documentos)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Escritura?> ObterCompletaPorNumeroOuProtocoloAsync(string numeroOuProtocolo, CancellationToken ct = default)
    {
        var termo = numeroOuProtocolo.Trim().ToUpperInvariant();
        return await DbSet
            .Include(e => e.Cartorio)
            .Include(e => e.Ato)
            .Include(e => e.Protocolo)
            .Include(e => e.Funcionario)
            .Include(e => e.Outorgante).ThenInclude(c => c!.Conjuge)
            .Include(e => e.Outorgante).ThenInclude(c => c!.CertidaoCasamento)
            .Include(e => e.Outorgado).ThenInclude(c => c!.Conjuge)
            .Include(e => e.Outorgado).ThenInclude(c => c!.CertidaoCasamento)
            .Include(e => e.Imovel)
            .Include(e => e.CertidaoOutroOrgao)
            .Include(e => e.Documentos)
            .FirstOrDefaultAsync(
                e => e.NumeroEscritura.ToUpper() == termo ||
                     (e.Protocolo != null && e.Protocolo.NumeroProtocolo.ToUpper() == termo) ||
                     e.Id.ToString() == termo ||
                     e.IdProtocolo.ToString() == termo,
                ct);
    }

    public async Task<bool> ExisteParaProtocoloAsync(int idProtocolo, int? ignorarId = null, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().AnyAsync(
            e => e.IdProtocolo == idProtocolo && (ignorarId == null || e.Id != ignorarId.Value),
            ct);

    public async Task<int> ObterProximoSequencialEscrituraAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().CountAsync(ct) + 1;

    public void AdicionarImovel(Imovel imovel) => Context.Imoveis.Add(imovel);

    public void AdicionarCertidaoOutroOrgao(CertidaoOutroOrgao certidao) => Context.CertidoesOutrosOrgaos.Add(certidao);

    public void AdicionarDocumento(DocumentoDigitalizado documento) => Context.DocumentosDigitalizados.Add(documento);

    public async Task<DocumentoDigitalizado?> ObterDocumentoPorIdAsync(int idDocumento, CancellationToken ct = default) =>
        await Context.DocumentosDigitalizados.FirstOrDefaultAsync(d => d.Id == idDocumento, ct);

    public void RemoverDocumento(DocumentoDigitalizado documento) =>
        Context.DocumentosDigitalizados.Remove(documento);
}

public sealed class BackupRepository(CartorioTccContext context)
    : BaseRepository<BackupRegistro>(context), IBackupRepository
{
    public async Task<IReadOnlyList<BackupRegistro>> ObterAgendadosPendentesAsync(DateTime ateData, CancellationToken ct = default) =>
        await DbSet
            .Where(b => !b.Executado && b.DataAgendada != null && b.DataAgendada <= ateData)
            .ToListAsync(ct);
}
