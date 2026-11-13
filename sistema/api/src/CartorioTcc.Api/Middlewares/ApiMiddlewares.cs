using System.Net;
using System.Text.Json;
using CartorioTcc.CrossCutting.IoC;

namespace CartorioTcc.Api.Middlewares;

public sealed class UserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, MockUserContext userContext)
    {
        if (context.Request.Headers.TryGetValue("X-Usuario-Login", out var login) && !string.IsNullOrWhiteSpace(login))
            userContext.Login = login.ToString();

        if (context.Request.Headers.TryGetValue("X-Usuario-Nome", out var nome) && !string.IsNullOrWhiteSpace(nome))
            userContext.Nome = nome.ToString();

        if (context.Request.Headers.TryGetValue("X-Usuario-Perfil", out var perfil) && !string.IsNullOrWhiteSpace(perfil))
            userContext.Perfis = [perfil.ToString()];

        if (context.Request.Headers.TryGetValue("X-Usuario-Id", out var idStr) && int.TryParse(idStr, out var id))
            userContext.FuncionarioId = id;

        await next(context);
    }
}

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado processando requisição {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                sucesso = false,
                erros = new[] { "Ocorreu um erro interno inesperado no servidor SAEC. Tente novamente." },
                detalhe = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
