using CartorioTcc.Business.Core.Interfaces;

namespace CartorioTcc.CrossCutting.IoC;

/// <summary>
/// Contexto de usuário de escopo da requisição.
/// Ressalva de Arquitetura: Como a autenticação está temporariamente inativa (bypass),
/// fornece um usuário default/mock ("admin@saec.com") para alimentar os campos de auditoria
/// (LOGIN_USUARIO_CRIACAO / LOGIN_USUARIO_ALTERACAO) de todas as AuditableEntity.
/// </summary>
public sealed class MockUserContext : IUserContext
{
    public string Login { get; set; } = "admin@saec.com";
    public string Nome { get; set; } = "Administrador SAEC";
    public int? FuncionarioId { get; set; } = 1;
    public bool Autenticado => true;
    public IReadOnlyCollection<string> Perfis { get; set; } = ["Administrador"];
}
