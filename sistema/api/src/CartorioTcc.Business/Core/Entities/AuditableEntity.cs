namespace CartorioTcc.Business.Core.Entities;

/// <summary>
/// Entidade auditável: rastreia DT_CRIACAO, LOGIN_USUARIO_CRIACAO, DT_ALTERACAO e LOGIN_USUARIO_ALTERACAO.
/// Os campos são preenchidos automaticamente pelo DbContext a partir do <c>IUserContext</c>
/// (enquanto a autenticação estiver inativa, é usado um usuário "mock" configurável).
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime DtCriacao { get; private set; }
    public string LoginUsuarioCriacao { get; private set; } = string.Empty;
    public DateTime? DtAlteracao { get; private set; }
    public string? LoginUsuarioAlteracao { get; private set; }

    public void RegistrarCriacao(string login, DateTime data)
    {
        DtCriacao = data;
        LoginUsuarioCriacao = login;
    }

    public void RegistrarAlteracao(string login, DateTime data)
    {
        DtAlteracao = data;
        LoginUsuarioAlteracao = login;
    }
}
