using Microsoft.AspNetCore.Mvc;
using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Core.Notifications;

namespace CartorioTcc.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseApiController(INotificadorService notificador) : ControllerBase
{
    protected INotificadorService Notificador => notificador;

    protected bool OperacaoValida() => !notificador.TemErros;

    protected IActionResult CustomResponse(object? result = null)
    {
        var erros = notificador.Notificacoes
            .Where(n => n.Tipo == TipoNotificacao.Erro)
            .Select(n => n.Mensagem)
            .ToList();

        var alertas = notificador.Notificacoes
            .Where(n => n.Tipo == TipoNotificacao.Alerta)
            .Select(n => n.Mensagem)
            .ToList();

        var sucessos = notificador.Notificacoes
            .Where(n => n.Tipo == TipoNotificacao.Sucesso)
            .Select(n => n.Mensagem)
            .ToList();

        if (notificador.RecursoNaoEncontrado)
        {
            return NotFound(new
            {
                sucesso = false,
                erros,
                alertas
            });
        }

        if (!OperacaoValida())
        {
            return BadRequest(new
            {
                sucesso = false,
                erros,
                alertas
            });
        }

        return Ok(new
        {
            sucesso = true,
            dados = result,
            mensagens = sucessos,
            alertas
        });
    }

    protected IActionResult CustomFileResponse(ArquivoGerado? arquivo)
    {
        if (!OperacaoValida() || arquivo is null)
            return CustomResponse();

        return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
    }
}
