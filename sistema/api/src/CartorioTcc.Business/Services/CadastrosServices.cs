using System.Security.Cryptography;
using System.Text;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Core.Notifications;
using CartorioTcc.Business.Core.Validations;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Business.Resources;

namespace CartorioTcc.Business.Services;

/// <summary>
/// Classe base dos serviços de negócio: utiliza o Notification Pattern via <c>INotificadorService</c>
/// (regras de negócio não lançam exceptions).
/// </summary>
public abstract class BaseService(INotificadorService notificador)
{
    protected INotificadorService Notificador { get; } = notificador;

    protected void NotificarErro(string mensagem, string? campo = null) =>
        Notificador.NotificarErro(mensagem, campo);

    protected void NotificarAlerta(string mensagem, string? campo = null) =>
        Notificador.NotificarAlerta(mensagem, campo);

    protected void NotificarSucesso(string mensagem) =>
        Notificador.NotificarSucesso(mensagem);

    protected void NotificarNaoEncontrado(string mensagem) =>
        Notificador.NotificarNaoEncontrado(mensagem);

    protected bool OperacaoValida => !Notificador.TemErros;

    protected static string GerarHashSha256(string texto)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        return Convert.ToHexString(bytes);
    }

    protected static string GerarHashSha256(byte[] dados) =>
        Convert.ToHexString(SHA256.HashData(dados));
}

/// <summary>
/// UC-01: Gerenciar Permissões de Acesso (associação Perfil x Página).
/// </summary>
public sealed class PermissaoService(
    INotificadorService notificador,
    IPermissaoRepository repository,
    ICatalogoQueryService queryService) : BaseService(notificador), IPermissaoService
{
    public async Task<PermissaoDto?> IncluirAsync(SalvarPermissaoCommand command, CancellationToken ct = default)
    {
        if (command.IdPerfil <= 0 || command.IdPagina <= 0)
        {
            NotificarErro(Mensagens.CamposObrigatorios);
            return null;
        }

        if (await repository.ExistePermissaoAsync(command.IdPerfil, command.IdPagina, null, ct))
        {
            NotificarErro(Mensagens.PermissaoDuplicada);
            return null;
        }

        var entidade = new PaginaPerfil(
            command.IdPerfil,
            command.IdPagina,
            command.PodeVisualizar,
            command.PodeIncluir,
            command.PodeEditar,
            command.PodeExcluir);

        repository.Adicionar(entidade);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.PermissaoIncluidaSucesso);
        return await queryService.ObterPermissaoPorIdAsync(entidade.Id, ct);
    }

    public async Task<PermissaoDto?> AlterarAsync(int id, SalvarPermissaoCommand command, CancellationToken ct = default)
    {
        if (command.IdPerfil <= 0 || command.IdPagina <= 0)
        {
            NotificarErro(Mensagens.CamposObrigatorios);
            return null;
        }

        var entidade = await repository.ObterPorIdAsync(id, ct);
        if (entidade is null)
        {
            NotificarNaoEncontrado(Mensagens.PermissaoNaoEncontrada);
            return null;
        }

        if (await repository.ExistePermissaoAsync(command.IdPerfil, command.IdPagina, id, ct))
        {
            NotificarErro(Mensagens.PermissaoDuplicada);
            return null;
        }

        entidade.Atualizar(
            command.IdPerfil,
            command.IdPagina,
            command.PodeVisualizar,
            command.PodeIncluir,
            command.PodeEditar,
            command.PodeExcluir,
            command.Status);

        repository.Atualizar(entidade);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.PermissaoAlteradaSucesso);
        return await queryService.ObterPermissaoPorIdAsync(entidade.Id, ct);
    }

    public async Task<bool> ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await repository.ObterPorIdAsync(id, ct);
        if (entidade is null)
        {
            NotificarNaoEncontrado(Mensagens.PermissaoNaoEncontrada);
            return false;
        }

        repository.Remover(entidade);
        await repository.UnitOfWork.CommitAsync(ct);
        NotificarSucesso(Mensagens.PermissaoExcluidaSucesso);
        return true;
    }
}

/// <summary>
/// UC-02: Gerenciar Cadastro de Funcionários.
/// </summary>
public sealed class FuncionarioService(
    INotificadorService notificador,
    IFuncionarioRepository repository,
    IFuncionarioQueryService queryService) : BaseService(notificador), IFuncionarioService
{
    public async Task<FuncionarioDetalheDto?> IncluirAsync(IncluirFuncionarioCommand command, CancellationToken ct = default)
    {
        ValidarDadosComuns(
            command.IdPerfil,
            command.NomeFuncionario,
            command.EmailFuncionario,
            command.CpfFuncionario,
            command.RgFuncionario,
            command.Endereco);

        if (!Validador.SenhaValida(command.Senha))
            NotificarErro(Mensagens.SenhaInvalida, nameof(command.Senha));

        if (!string.Equals(command.Senha, command.ConfirmarSenha, StringComparison.Ordinal))
            NotificarErro(Mensagens.SenhasNaoConferem, nameof(command.ConfirmarSenha));

        if (!OperacaoValida) return null;

        var cpfLimpo = Validador.SomenteNumeros(command.CpfFuncionario);
        if (await repository.ExisteCpfOuEmailAsync(cpfLimpo, command.EmailFuncionario.Trim(), null, ct))
        {
            NotificarErro(Mensagens.FuncionarioJaCadastrado, nameof(command.CpfFuncionario));
            return null;
        }

        var funcionario = new Funcionario(
            command.IdPerfil,
            command.NomeFuncionario,
            command.DataNascFuncionario,
            command.DataAdmissao,
            command.EmailFuncionario,
            GerarHashSha256(command.Senha),
            cpfLimpo,
            command.RgFuncionario,
            command.TelefoneFuncionario,
            command.Cargo,
            command.Endereco.ParaDomain(),
            command.StatusFuncionario);

        repository.Adicionar(funcionario);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.FuncionarioInseridoSucesso);
        return await queryService.ObterPorIdAsync(funcionario.Id, ct);
    }

    public async Task<FuncionarioDetalheDto?> AlterarAsync(AlterarFuncionarioCommand command, CancellationToken ct = default)
    {
        ValidarDadosComuns(
            command.IdPerfil,
            command.NomeFuncionario,
            command.EmailFuncionario,
            command.CpfFuncionario,
            command.RgFuncionario,
            command.Endereco);

        if (!string.IsNullOrWhiteSpace(command.NovaSenha))
        {
            if (!Validador.SenhaValida(command.NovaSenha))
                NotificarErro(Mensagens.SenhaInvalida, nameof(command.NovaSenha));

            if (!string.Equals(command.NovaSenha, command.ConfirmarNovaSenha, StringComparison.Ordinal))
                NotificarErro(Mensagens.SenhasNaoConferem, nameof(command.ConfirmarNovaSenha));
        }

        if (!OperacaoValida) return null;

        var funcionario = await repository.ObterPorIdAsync(command.Id, ct);
        if (funcionario is null)
        {
            NotificarNaoEncontrado(Mensagens.FuncionarioNaoEncontrado);
            return null;
        }

        var cpfLimpo = Validador.SomenteNumeros(command.CpfFuncionario);
        if (await repository.ExisteCpfOuEmailAsync(cpfLimpo, command.EmailFuncionario.Trim(), command.Id, ct))
        {
            NotificarErro(Mensagens.FuncionarioJaCadastrado, nameof(command.CpfFuncionario));
            return null;
        }

        funcionario.AtualizarDados(
            command.IdPerfil,
            command.NomeFuncionario,
            command.DataNascFuncionario,
            command.DataAdmissao,
            command.EmailFuncionario,
            cpfLimpo,
            command.RgFuncionario,
            command.TelefoneFuncionario,
            command.Cargo,
            command.Endereco.ParaDomain(),
            command.StatusFuncionario);

        if (!string.IsNullOrWhiteSpace(command.NovaSenha))
            funcionario.AlterarSenha(GerarHashSha256(command.NovaSenha));

        repository.Atualizar(funcionario);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.FuncionarioAlteradoSucesso);
        return await queryService.ObterPorIdAsync(funcionario.Id, ct);
    }

    public async Task<bool> InativarAsync(int id, CancellationToken ct = default)
    {
        var funcionario = await repository.ObterPorIdAsync(id, ct);
        if (funcionario is null)
        {
            NotificarNaoEncontrado(Mensagens.FuncionarioNaoEncontrado);
            return false;
        }

        funcionario.Inativar();
        repository.Atualizar(funcionario);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.FuncionarioExcluidoSucesso);
        return true;
    }

    private void ValidarDadosComuns(
        int idPerfil,
        string? nome,
        string? email,
        string? cpf,
        string? rg,
        EnderecoDto? endereco)
    {
        if (idPerfil <= 0 ||
            string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(cpf) ||
            string.IsNullOrWhiteSpace(rg) ||
            endereco is null ||
            string.IsNullOrWhiteSpace(endereco.Cep) ||
            string.IsNullOrWhiteSpace(endereco.Logradouro) ||
            string.IsNullOrWhiteSpace(endereco.Cidade) ||
            string.IsNullOrWhiteSpace(endereco.Estado))
        {
            NotificarErro(Mensagens.CamposObrigatorios);
            return;
        }

        if (!Validador.NomeValido(nome))
            NotificarErro(Mensagens.NomeInvalido, nameof(IncluirFuncionarioCommand.NomeFuncionario));

        if (!Validador.CpfValido(cpf))
            NotificarErro(Mensagens.CpfInvalido, nameof(IncluirFuncionarioCommand.CpfFuncionario));

        if (!Validador.EmailValido(email))
            NotificarErro(Mensagens.EmailInvalido, nameof(IncluirFuncionarioCommand.EmailFuncionario));

        if (!Validador.CepValido(endereco.Cep))
            NotificarErro(Mensagens.CepInvalido, nameof(EnderecoDto.Cep));
    }
}

/// <summary>
/// UC-08: Gerenciar Dados de Clientes (inclui Cônjuge e Certidão de Casamento).
/// </summary>
public sealed class ClienteService(
    INotificadorService notificador,
    IClienteRepository repository,
    IClienteQueryService queryService,
    IDateTimeProvider dateTimeProvider) : BaseService(notificador), IClienteService
{
    public async Task<ClienteDetalheDto?> IncluirAsync(SalvarClienteCommand command, CancellationToken ct = default)
    {
        ValidarCliente(command);
        if (!OperacaoValida) return null;

        var cpfLimpo = Validador.SomenteNumeros(command.CpfCliente);
        if (await repository.ExisteCpfAsync(cpfLimpo, null, ct))
        {
            NotificarErro(Mensagens.ClienteJaCadastrado, nameof(command.CpfCliente));
            return null;
        }

        var cliente = new Cliente(
            command.NomeCliente,
            command.RgCliente,
            command.OrgaoExpedidorCliente,
            command.NacionalidadeCliente,
            command.EstadoOrgaoExpedidorCliente,
            cpfLimpo,
            command.DataNascCliente,
            command.EmailCliente,
            command.GeneroCliente,
            command.EstadoCivilCliente,
            command.RegimeBens,
            command.ProfissaoCliente,
            command.TelefoneCliente,
            command.Endereco.ParaDomain(),
            command.StatusCliente);

        AplicarConjugeECertidao(cliente, command);

        repository.Adicionar(cliente);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.ClienteInseridoSucesso);
        return await queryService.ObterPorIdAsync(cliente.Id, ct);
    }

    public async Task<ClienteDetalheDto?> AlterarAsync(int id, SalvarClienteCommand command, CancellationToken ct = default)
    {
        ValidarCliente(command);
        if (!OperacaoValida) return null;

        var cliente = await repository.ObterCompletoPorIdAsync(id, ct);
        if (cliente is null)
        {
            NotificarNaoEncontrado(Mensagens.ClienteNaoEncontrado);
            return null;
        }

        var cpfLimpo = Validador.SomenteNumeros(command.CpfCliente);
        if (await repository.ExisteCpfAsync(cpfLimpo, id, ct))
        {
            NotificarErro(Mensagens.ClienteJaCadastrado, nameof(command.CpfCliente));
            return null;
        }

        cliente.Atualizar(
            command.NomeCliente,
            command.RgCliente,
            command.OrgaoExpedidorCliente,
            command.NacionalidadeCliente,
            command.EstadoOrgaoExpedidorCliente,
            cpfLimpo,
            command.DataNascCliente,
            command.EmailCliente,
            command.GeneroCliente,
            command.EstadoCivilCliente,
            command.RegimeBens,
            command.ProfissaoCliente,
            command.TelefoneCliente,
            command.Endereco.ParaDomain(),
            command.StatusCliente);

        AplicarConjugeECertidao(cliente, command);

        repository.Atualizar(cliente);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.ClienteAlteradoSucesso);
        return await queryService.ObterPorIdAsync(cliente.Id, ct);
    }

    public async Task<bool> InativarAsync(int id, CancellationToken ct = default)
    {
        var cliente = await repository.ObterPorIdAsync(id, ct);
        if (cliente is null)
        {
            NotificarNaoEncontrado(Mensagens.ClienteNaoEncontrado);
            return false;
        }

        cliente.Inativar();
        repository.Atualizar(cliente);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.ClienteExcluidoSucesso);
        return true;
    }

    private void ValidarCliente(SalvarClienteCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.NomeCliente) ||
            string.IsNullOrWhiteSpace(command.RgCliente) ||
            string.IsNullOrWhiteSpace(command.OrgaoExpedidorCliente) ||
            string.IsNullOrWhiteSpace(command.CpfCliente) ||
            string.IsNullOrWhiteSpace(command.EmailCliente) ||
            string.IsNullOrWhiteSpace(command.TelefoneCliente) ||
            string.IsNullOrWhiteSpace(command.ProfissaoCliente) ||
            command.Endereco is null ||
            string.IsNullOrWhiteSpace(command.Endereco.Cep) ||
            string.IsNullOrWhiteSpace(command.Endereco.Logradouro) ||
            string.IsNullOrWhiteSpace(command.Endereco.Cidade) ||
            string.IsNullOrWhiteSpace(command.Endereco.Estado))
        {
            NotificarErro(Mensagens.CamposObrigatorios);
            return;
        }

        if (!Validador.NomeValido(command.NomeCliente))
            NotificarErro(Mensagens.NomeInvalido, nameof(command.NomeCliente));

        if (!Validador.CpfValido(command.CpfCliente))
            NotificarErro(Mensagens.CpfInvalido, nameof(command.CpfCliente));

        if (!Validador.EmailValido(command.EmailCliente))
            NotificarErro(Mensagens.EmailInvalido, nameof(command.EmailCliente));

        if (!Validador.TelefoneValido(command.TelefoneCliente))
            NotificarErro(Mensagens.TelefoneInvalido, nameof(command.TelefoneCliente));

        if (!Validador.CepValido(command.Endereco.Cep))
            NotificarErro(Mensagens.CepInvalido, nameof(EnderecoDto.Cep));

        if (!Validador.MaiorDeIdade(command.DataNascCliente, dateTimeProvider.Hoje))
            NotificarErro(Mensagens.ClienteMenorDeIdade, nameof(command.DataNascCliente));

        if (command.Conjuge is not null && !string.IsNullOrWhiteSpace(command.Conjuge.NomeConjuge))
        {
            if (!Validador.CpfValido(command.Conjuge.CpfConjuge))
                NotificarErro(Mensagens.CpfInvalido, "Conjuge.CpfConjuge");
        }
    }

    private static void AplicarConjugeECertidao(Cliente cliente, SalvarClienteCommand command)
    {
        if (command.Conjuge is not null && !string.IsNullOrWhiteSpace(command.Conjuge.NomeConjuge))
        {
            if (cliente.Conjuge is null)
            {
                cliente.DefinirConjuge(new Conjuge(
                    cliente.Id,
                    command.Conjuge.NomeConjuge,
                    command.Conjuge.RgConjuge,
                    command.Conjuge.OrgaoExpedidorConjuge,
                    command.Conjuge.NacionalidadeConjuge,
                    command.Conjuge.EstadoOrgaoExpedidorConjuge,
                    command.Conjuge.CpfConjuge,
                    command.Conjuge.DataNascConjuge,
                    command.Conjuge.EmailConjuge,
                    command.Conjuge.GeneroConjuge,
                    command.Conjuge.EstadoCivilConjuge,
                    command.Conjuge.ProfissaoConjuge,
                    command.Conjuge.TelefoneConjuge,
                    command.Conjuge.Endereco.ParaDomain()));
            }
            else
            {
                cliente.Conjuge.Atualizar(
                    command.Conjuge.NomeConjuge,
                    command.Conjuge.RgConjuge,
                    command.Conjuge.OrgaoExpedidorConjuge,
                    command.Conjuge.NacionalidadeConjuge,
                    command.Conjuge.EstadoOrgaoExpedidorConjuge,
                    command.Conjuge.CpfConjuge,
                    command.Conjuge.DataNascConjuge,
                    command.Conjuge.EmailConjuge,
                    command.Conjuge.GeneroConjuge,
                    command.Conjuge.EstadoCivilConjuge,
                    command.Conjuge.ProfissaoConjuge,
                    command.Conjuge.TelefoneConjuge,
                    command.Conjuge.Endereco.ParaDomain());
            }
        }

        if (command.CertidaoCasamento is not null &&
            !string.IsNullOrWhiteSpace(command.CertidaoCasamento.CartorioCertidaoCasamento))
        {
            if (cliente.CertidaoCasamento is null)
            {
                cliente.DefinirCertidaoCasamento(new CertidaoCasamento(
                    cliente.Id,
                    cliente.Conjuge?.Id,
                    command.CertidaoCasamento.CartorioCertidaoCasamento,
                    command.CertidaoCasamento.LivroCertidaoCasamento,
                    command.CertidaoCasamento.FolhasCertidaoCasamento,
                    command.CertidaoCasamento.TermoCertidaoCasamento,
                    command.CertidaoCasamento.DataCertidaoCasamento,
                    command.CertidaoCasamento.Averbacoes));
            }
            else
            {
                cliente.CertidaoCasamento.Atualizar(
                    cliente.Conjuge?.Id,
                    command.CertidaoCasamento.CartorioCertidaoCasamento,
                    command.CertidaoCasamento.LivroCertidaoCasamento,
                    command.CertidaoCasamento.FolhasCertidaoCasamento,
                    command.CertidaoCasamento.TermoCertidaoCasamento,
                    command.CertidaoCasamento.DataCertidaoCasamento,
                    command.CertidaoCasamento.Averbacoes);
            }
        }
    }
}
