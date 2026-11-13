using CartorioTcc.Business.Builders;
using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Core.Validations;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Enums;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CartorioTcc.Infra.Data.Queries;

public sealed class CatalogoQueryService(CartorioTccContext context) : ICatalogoQueryService
{
    public async Task<IReadOnlyList<PerfilDto>> ListarPerfisAsync(CancellationToken ct = default) =>
        await context.Perfis.AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new PerfilDto(p.Id, p.NomePerfil, p.Descricao, p.Status))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PaginaDto>> ListarPaginasAsync(CancellationToken ct = default) =>
        await context.Paginas.AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new PaginaDto(p.Id, p.NomePagina, p.Url, p.Modulo))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PermissaoDto>> ListarPermissoesAsync(int? idPerfil = null, CancellationToken ct = default)
    {
        var query = context.PaginasPerfis.AsNoTracking()
            .Include(pp => pp.Perfil)
            .Include(pp => pp.Pagina)
            .AsQueryable();

        if (idPerfil is > 0)
            query = query.Where(pp => pp.IdPerfil == idPerfil.Value);

        return await query
            .OrderBy(pp => pp.IdPerfil)
            .ThenBy(pp => pp.IdPagina)
            .Select(pp => new PermissaoDto(
                pp.Id,
                pp.IdPerfil,
                pp.Perfil != null ? pp.Perfil.NomePerfil : string.Empty,
                pp.IdPagina,
                pp.Pagina != null ? pp.Pagina.NomePagina : string.Empty,
                pp.Pagina != null ? pp.Pagina.Url : string.Empty,
                pp.Pagina != null ? pp.Pagina.Modulo : null,
                pp.PodeVisualizar,
                pp.PodeIncluir,
                pp.PodeEditar,
                pp.PodeExcluir,
                pp.Status))
            .ToListAsync(ct);
    }

    public async Task<PermissaoDto?> ObterPermissaoPorIdAsync(int id, CancellationToken ct = default) =>
        await context.PaginasPerfis.AsNoTracking()
            .Where(pp => pp.Id == id)
            .Select(pp => new PermissaoDto(
                pp.Id,
                pp.IdPerfil,
                pp.Perfil != null ? pp.Perfil.NomePerfil : string.Empty,
                pp.IdPagina,
                pp.Pagina != null ? pp.Pagina.NomePagina : string.Empty,
                pp.Pagina != null ? pp.Pagina.Url : string.Empty,
                pp.Pagina != null ? pp.Pagina.Modulo : null,
                pp.PodeVisualizar,
                pp.PodeIncluir,
                pp.PodeEditar,
                pp.PodeExcluir,
                pp.Status))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CartorioDto>> ListarCartoriosAsync(CancellationToken ct = default) =>
        await context.Cartorios.AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CartorioDto(
                c.Id,
                c.NomeCartorio,
                c.CnpjCartorio,
                c.TabeliaoResponsavel,
                c.EnderecoCompleto,
                c.Telefone,
                c.Email,
                c.CodigoCns))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AtoDto>> ListarAtosAsync(CancellationToken ct = default) =>
        await context.Atos.AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => new AtoDto(
                a.Id,
                a.TipoAto,
                a.DescricaoAto,
                a.ModeloMinutaPadrao,
                a.ExigeImovel,
                a.GeraDoi,
                a.Ativo))
            .ToListAsync(ct);
}

public sealed class FuncionarioQueryService(CartorioTccContext context) : IFuncionarioQueryService
{
    public async Task<ResultadoPaginado<FuncionarioResumoDto>> PesquisarAsync(FiltroFuncionarioDto filtro, CancellationToken ct = default)
    {
        var query = context.Funcionarios.AsNoTracking()
            .Include(f => f.Perfil)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var t = filtro.Termo.Trim().ToLowerInvariant();
            var num = Validador.SomenteNumeros(t);
            query = query.Where(f =>
                f.NomeFuncionario.ToLower().Contains(t) ||
                f.EmailFuncionario.ToLower().Contains(t) ||
                (!string.IsNullOrEmpty(num) && f.CpfFuncionario.Contains(num)));
        }

        if (filtro.IdPerfil is > 0)
            query = query.Where(f => f.IdPerfil == filtro.IdPerfil.Value);

        if (filtro.Status.HasValue)
            query = query.Where(f => f.StatusFuncionario == filtro.Status.Value);

        var total = await query.CountAsync(ct);

        var itens = await query
            .OrderBy(f => f.NomeFuncionario)
            .Skip(filtro.Skip)
            .Take(filtro.Tamanho)
            .Select(f => new FuncionarioResumoDto(
                f.Id,
                f.NomeFuncionario,
                f.EmailFuncionario,
                f.CpfFuncionario,
                f.RgFuncionario,
                f.IdPerfil,
                f.Perfil != null ? f.Perfil.NomePerfil : string.Empty,
                f.Cargo,
                f.TelefoneFuncionario,
                f.DataAdmissao,
                f.StatusFuncionario,
                $"{f.Endereco.Cidade}/{f.Endereco.Estado}"))
            .ToListAsync(ct);

        return new ResultadoPaginado<FuncionarioResumoDto>(itens, total, filtro.Pagina, filtro.Tamanho);
    }

    public async Task<FuncionarioDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var f = await context.Funcionarios.AsNoTracking()
            .Include(x => x.Perfil)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (f is null) return null;

        return new FuncionarioDetalheDto(
            f.Id,
            f.IdPerfil,
            f.Perfil?.NomePerfil ?? string.Empty,
            f.NomeFuncionario,
            f.DataNascFuncionario,
            f.DataAdmissao,
            f.EmailFuncionario,
            f.CpfFuncionario,
            f.RgFuncionario,
            f.TelefoneFuncionario,
            f.Cargo,
            f.StatusFuncionario,
            EnderecoDto.DeDomain(f.Endereco),
            f.DtCriacao,
            f.DtAlteracao);
    }

    public async Task<IReadOnlyList<OpcaoDto>> ListarOpcoesAtivasAsync(CancellationToken ct = default) =>
        await context.Funcionarios.AsNoTracking()
            .Where(f => f.StatusFuncionario == StatusRegistro.Ativo)
            .OrderBy(f => f.NomeFuncionario)
            .Select(f => new OpcaoDto(f.Id, f.NomeFuncionario, f.Cargo))
            .ToListAsync(ct);
}

public sealed class ClienteQueryService(CartorioTccContext context) : IClienteQueryService
{
    public async Task<ResultadoPaginado<ClienteResumoDto>> PesquisarAsync(FiltroClienteDto filtro, CancellationToken ct = default)
    {
        var query = context.Clientes.AsNoTracking()
            .Include(c => c.Conjuge)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var t = filtro.Termo.Trim().ToLowerInvariant();
            var num = Validador.SomenteNumeros(t);
            query = query.Where(c =>
                c.NomeCliente.ToLower().Contains(t) ||
                c.EmailCliente.ToLower().Contains(t) ||
                (!string.IsNullOrEmpty(num) && c.CpfCliente.Contains(num)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Cpf))
        {
            var cpfNum = Validador.SomenteNumeros(filtro.Cpf);
            query = query.Where(c => c.CpfCliente == cpfNum);
        }

        if (filtro.Status.HasValue)
            query = query.Where(c => c.StatusCliente == filtro.Status.Value);

        var total = await query.CountAsync(ct);

        var itens = await query
            .OrderBy(c => c.NomeCliente)
            .Skip(filtro.Skip)
            .Take(filtro.Tamanho)
            .Select(c => new ClienteResumoDto(
                c.Id,
                c.NomeCliente,
                c.CpfCliente,
                c.RgCliente,
                c.EmailCliente,
                c.TelefoneCliente,
                c.EstadoCivilCliente,
                c.ProfissaoCliente,
                $"{c.Endereco.Cidade}/{c.Endereco.Estado}",
                c.Conjuge != null ? c.Conjuge.NomeConjuge : null,
                c.StatusCliente))
            .ToListAsync(ct);

        return new ResultadoPaginado<ClienteResumoDto>(itens, total, filtro.Pagina, filtro.Tamanho);
    }

    public async Task<ClienteDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var c = await context.Clientes.AsNoTracking()
            .Include(x => x.Conjuge)
            .Include(x => x.CertidaoCasamento)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return c is null ? null : MapearClienteDetalhe(c);
    }

    public async Task<ClienteDetalheDto?> ObterPorCpfAsync(string cpf, CancellationToken ct = default)
    {
        var cpfNum = Validador.SomenteNumeros(cpf);
        var c = await context.Clientes.AsNoTracking()
            .Include(x => x.Conjuge)
            .Include(x => x.CertidaoCasamento)
            .FirstOrDefaultAsync(x => x.CpfCliente == cpfNum, ct);

        return c is null ? null : MapearClienteDetalhe(c);
    }

    public async Task<IReadOnlyList<OpcaoDto>> ListarOpcoesAtivasAsync(string? termo = null, CancellationToken ct = default)
    {
        var query = context.Clientes.AsNoTracking()
            .Where(c => c.StatusCliente == StatusRegistro.Ativo);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim().ToLowerInvariant();
            var num = Validador.SomenteNumeros(t);
            query = query.Where(c =>
                c.NomeCliente.ToLower().Contains(t) ||
                (!string.IsNullOrEmpty(num) && c.CpfCliente.Contains(num)));
        }

        return await query
            .OrderBy(c => c.NomeCliente)
            .Take(50)
            .Select(c => new OpcaoDto(c.Id, c.NomeCliente, c.CpfCliente))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DocumentoDigitalizadoDto>> ConsultarDocumentosPorClienteAsync(string termoNomeOuCpf, CancellationToken ct = default)
    {
        var t = (termoNomeOuCpf ?? string.Empty).Trim().ToLowerInvariant();
        var num = Validador.SomenteNumeros(t);

        return await context.DocumentosDigitalizados.AsNoTracking()
            .Include(d => d.Cliente)
            .Include(d => d.Escritura).ThenInclude(e => e!.Outorgante)
            .Include(d => d.Escritura).ThenInclude(e => e!.Outorgado)
            .Where(d =>
                string.IsNullOrEmpty(t) ||
                (d.Cliente != null && (d.Cliente.NomeCliente.ToLower().Contains(t) || (!string.IsNullOrEmpty(num) && d.Cliente.CpfCliente.Contains(num)))) ||
                (d.Escritura != null && d.Escritura.Outorgante != null && (d.Escritura.Outorgante.NomeCliente.ToLower().Contains(t) || (!string.IsNullOrEmpty(num) && d.Escritura.Outorgante.CpfCliente.Contains(num)))) ||
                (d.Escritura != null && d.Escritura.Outorgado != null && (d.Escritura.Outorgado.NomeCliente.ToLower().Contains(t) || (!string.IsNullOrEmpty(num) && d.Escritura.Outorgado.CpfCliente.Contains(num)))))
            .OrderByDescending(d => d.DtCriacao)
            .Select(d => new DocumentoDigitalizadoDto(
                d.Id,
                d.IdEscritura,
                d.IdCliente ?? (d.Escritura != null ? d.Escritura.IdOutorgante : null),
                d.Cliente != null ? d.Cliente.NomeCliente : (d.Escritura != null && d.Escritura.Outorgante != null ? d.Escritura.Outorgante.NomeCliente : null),
                d.Cliente != null ? d.Cliente.CpfCliente : (d.Escritura != null && d.Escritura.Outorgante != null ? d.Escritura.Outorgante.CpfCliente : null),
                d.TipoDocumento,
                d.TipoDocumento.ToString(),
                d.NomeArquivo,
                d.ContentType,
                d.TamanhoBytes,
                d.HashSha256,
                d.Observacao,
                d.DtCriacao))
            .ToListAsync(ct);
    }

    internal static ClienteDetalheDto MapearClienteDetalhe(Cliente c) => new(
        c.Id,
        c.NomeCliente,
        c.RgCliente,
        c.OrgaoExpedidorCliente,
        c.NacionalidadeCliente,
        c.EstadoOrgaoExpedidorCliente,
        c.CpfCliente,
        c.DataNascCliente,
        c.EmailCliente,
        c.GeneroCliente,
        c.EstadoCivilCliente,
        c.RegimeBens,
        c.ProfissaoCliente,
        c.TelefoneCliente,
        c.StatusCliente,
        EnderecoDto.DeDomain(c.Endereco),
        c.Conjuge is null
            ? null
            : new ConjugeDto(
                c.Conjuge.Id,
                c.Conjuge.NomeConjuge,
                c.Conjuge.RgConjuge,
                c.Conjuge.OrgaoExpedidorConjuge,
                c.Conjuge.NacionalidadeConjuge,
                c.Conjuge.EstadoOrgaoExpedidorConjuge,
                c.Conjuge.CpfConjuge,
                c.Conjuge.DataNascConjuge,
                c.Conjuge.EmailConjuge,
                c.Conjuge.GeneroConjuge,
                c.Conjuge.EstadoCivilConjuge,
                c.Conjuge.ProfissaoConjuge,
                c.Conjuge.TelefoneConjuge,
                EnderecoDto.DeDomain(c.Conjuge.Endereco)),
        c.CertidaoCasamento is null
            ? null
            : new CertidaoCasamentoDto(
                c.CertidaoCasamento.Id,
                c.CertidaoCasamento.CartorioCertidaoCasamento,
                c.CertidaoCasamento.LivroCertidaoCasamento,
                c.CertidaoCasamento.FolhasCertidaoCasamento,
                c.CertidaoCasamento.TermoCertidaoCasamento,
                c.CertidaoCasamento.DataCertidaoCasamento,
                c.CertidaoCasamento.Averbacoes),
        c.DtCriacao,
        c.DtAlteracao);
}
