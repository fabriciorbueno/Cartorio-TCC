using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CartorioTcc.Api.Authorization;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Core.Notifications;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Interfaces;

namespace CartorioTcc.Api.Controllers;

[Route("api/catalogos")]
public sealed class CatalogosController(
    INotificadorService notificador,
    ICatalogoQueryService catalogoQuery,
    ICepExternalService cepExternalService,
    IUserContext userContext) : BaseApiController(notificador)
{
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() => CustomResponse(new
    {
        sistema = "SAEC — Sistema Automatizador de Escrituração para Cartórios",
        serventia = "2º Tabelionato de Notas de Campinas",
        versao = "2.0.0 (.NET 10 + Angular 21)",
        autenticacaoModo = "Bypass Temporário Ativo (Estrutura JWT pronta)",
        status = "Online",
        timestampUtc = DateTime.UtcNow
    });

    [HttpGet("usuario-atual")]
    [AllowAnonymous]
    public IActionResult ObterUsuarioAtual() => CustomResponse(new
    {
        login = userContext.Login,
        nome = userContext.Nome,
        perfis = userContext.Perfis,
        funcionarioId = userContext.FuncionarioId,
        autenticado = userContext.Autenticado
    });

    [HttpGet("perfis")]
    public async Task<IActionResult> ListarPerfis(CancellationToken ct)
        => CustomResponse(await catalogoQuery.ListarPerfisAsync(ct));

    [HttpGet("paginas")]
    public async Task<IActionResult> ListarPaginas(CancellationToken ct)
        => CustomResponse(await catalogoQuery.ListarPaginasAsync(ct));

    [HttpGet("cartorios")]
    public async Task<IActionResult> ListarCartorios(CancellationToken ct)
        => CustomResponse(await catalogoQuery.ListarCartoriosAsync(ct));

    [HttpGet("atos")]
    public async Task<IActionResult> ListarAtos(CancellationToken ct)
        => CustomResponse(await catalogoQuery.ListarAtosAsync(ct));

    [HttpGet("cep/{cep}")]
    public async Task<IActionResult> ConsultarCep(string cep, CancellationToken ct)
    {
        var endereco = await cepExternalService.ConsultarCepAsync(cep, ct);
        if (endereco is null)
        {
            Notificador.NotificarErro("CEP inválido. Informe um CEP contendo 8 dígitos numéricos.");
            return CustomResponse();
        }

        return CustomResponse(endereco);
    }
}

/// <summary>
/// UC-01: Gerenciar Permissões de Acesso por Perfil e Página
/// </summary>
[Route("api/permissoes")]
[Authorize(Policy = CartorioPolicies.Administracao)]
public sealed class PermissoesController(
    INotificadorService notificador,
    ICatalogoQueryService catalogoQuery,
    IPermissaoService permissaoService) : BaseApiController(notificador)
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? idPerfil, CancellationToken ct)
        => CustomResponse(await catalogoQuery.ListarPermissoesAsync(idPerfil, ct));

    [HttpGet("perfil/{idPerfil:int}")]
    public async Task<IActionResult> ObterPorPerfil(int idPerfil, CancellationToken ct)
        => CustomResponse(await catalogoQuery.ListarPermissoesAsync(idPerfil, ct));

    [HttpPost]
    public async Task<IActionResult> Incluir([FromBody] SalvarPermissaoCommand command, CancellationToken ct)
        => CustomResponse(await permissaoService.IncluirAsync(command, ct));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Alterar(int id, [FromBody] SalvarPermissaoCommand command, CancellationToken ct)
        => CustomResponse(await permissaoService.AlterarAsync(id, command, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
        => CustomResponse(await permissaoService.ExcluirAsync(id, ct));
}

/// <summary>
/// UC-02: Gerenciar Cadastro de Funcionários
/// </summary>
[Route("api/funcionarios")]
[Authorize(Policy = CartorioPolicies.Administracao)]
public sealed class FuncionariosController(
    INotificadorService notificador,
    IFuncionarioQueryService funcionarioQuery,
    IFuncionarioService funcionarioService) : BaseApiController(notificador)
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] FiltroFuncionarioDto filtro, CancellationToken ct)
        => CustomResponse(await funcionarioQuery.PesquisarAsync(filtro, ct));

    [HttpGet("opcoes")]
    public async Task<IActionResult> ListarOpcoes(CancellationToken ct)
        => CustomResponse(await funcionarioQuery.ListarOpcoesAtivasAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var dto = await funcionarioQuery.ObterPorIdAsync(id, ct);
        if (dto is null)
        {
            Notificador.NotificarNaoEncontrado("Funcionário não encontrado.");
            return CustomResponse();
        }
        return CustomResponse(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Incluir([FromBody] IncluirFuncionarioCommand command, CancellationToken ct)
        => CustomResponse(await funcionarioService.IncluirAsync(command, ct));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Alterar(int id, [FromBody] AlterarFuncionarioCommand command, CancellationToken ct)
        => CustomResponse(await funcionarioService.AlterarAsync(command with { Id = id }, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
        => CustomResponse(await funcionarioService.InativarAsync(id, ct));
}

/// <summary>
/// UC-08: Gerenciar Dados de Clientes (Dados Pessoais, Endereço, Cônjuge, Certidão de Casamento e Digitalizações)
/// </summary>
[Route("api/clientes")]
[Authorize(Policy = CartorioPolicies.Escrituracao)]
public sealed class ClientesController(
    INotificadorService notificador,
    IClienteQueryService clienteQuery,
    IClienteService clienteService) : BaseApiController(notificador)
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] FiltroClienteDto filtro, CancellationToken ct)
        => CustomResponse(await clienteQuery.PesquisarAsync(filtro, ct));

    [HttpGet("opcoes")]
    public async Task<IActionResult> ListarOpcoes([FromQuery] string? termo, CancellationToken ct)
        => CustomResponse(await clienteQuery.ListarOpcoesAtivasAsync(termo, ct));

    [HttpGet("documentos")]
    public async Task<IActionResult> ConsultarDocumentosPorCliente([FromQuery] string? termo, CancellationToken ct)
        => CustomResponse(await clienteQuery.ConsultarDocumentosPorClienteAsync(termo ?? string.Empty, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var dto = await clienteQuery.ObterPorIdAsync(id, ct);
        if (dto is null)
        {
            Notificador.NotificarNaoEncontrado("Cliente não encontrado.");
            return CustomResponse();
        }
        return CustomResponse(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Incluir([FromBody] SalvarClienteCommand command, CancellationToken ct)
        => CustomResponse(await clienteService.IncluirAsync(command, ct));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Alterar(int id, [FromBody] SalvarClienteCommand command, CancellationToken ct)
        => CustomResponse(await clienteService.AlterarAsync(id, command, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
        => CustomResponse(await clienteService.InativarAsync(id, ct));
}
