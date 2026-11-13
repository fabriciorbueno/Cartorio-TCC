using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace CartorioTcc.Api.Authorization;

/// <summary>
/// Estrutura base de Autenticação/Autorização (Políticas e Claims JWT).
/// Ressalva de Arquitetura: Criada e configurada nos Controllers, porém temporariamente
/// em modo Bypass (inativa) para permitir acesso irrestrito até a ativação do Login/Token corporativo.
/// </summary>
public static class CartorioPolicies
{
    public const string Administracao = "Policy.Administracao";
    public const string Tabelionato = "Policy.Tabelionato";
    public const string Escrituracao = "Policy.Escrituracao";
    public const string Atendimento = "Policy.Atendimento";
    public const string Relatorios = "Policy.Relatorios";
}

public static class CartorioClaims
{
    public const string Login = "saec:login";
    public const string Perfil = "saec:perfil";
    public const string CartorioId = "saec:cartorio_id";
    public const string PermissaoPagina = "saec:pagina";
}

public sealed class BypassAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "SaecBypassJwt";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var login = Request.Headers["X-Usuario-Login"].FirstOrDefault() ?? "admin@saec.com";
        var nome = Request.Headers["X-Usuario-Nome"].FirstOrDefault() ?? "Administrador SAEC";
        var perfil = Request.Headers["X-Usuario-Perfil"].FirstOrDefault() ?? "Administrador";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, nome),
            new Claim(ClaimTypes.Email, login),
            new Claim(ClaimTypes.Role, perfil),
            new Claim(CartorioClaims.Login, login),
            new Claim(CartorioClaims.Perfil, perfil),
            new Claim(CartorioClaims.CartorioId, "1")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public sealed class BypassAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class AuthorizationSetupExtensions
{
    public static IServiceCollection AddCartorioSecurityStructure(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = BypassAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = BypassAuthenticationHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, BypassAuthenticationHandler>(
                BypassAuthenticationHandler.SchemeName, _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy(CartorioPolicies.Administracao, p => p.RequireAuthenticatedUser())
            .AddPolicy(CartorioPolicies.Tabelionato, p => p.RequireAuthenticatedUser())
            .AddPolicy(CartorioPolicies.Escrituracao, p => p.RequireAuthenticatedUser())
            .AddPolicy(CartorioPolicies.Atendimento, p => p.RequireAuthenticatedUser())
            .AddPolicy(CartorioPolicies.Relatorios, p => p.RequireAuthenticatedUser());

        services.AddSingleton<IAuthorizationHandler, BypassAuthorizationHandler>();

        return services;
    }
}
