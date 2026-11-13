namespace CartorioTcc.Business.Core.Notifications;

public enum TipoNotificacao
{
    Erro = 1,
    Alerta = 2,
    Sucesso = 3
}

public sealed record Notificacao(TipoNotificacao Tipo, string Mensagem, string? Campo = null);

/// <summary>
/// Notification Pattern: regras de negócio não disparam exceptions.
/// Os serviços notificam e o <c>BaseApiController.CustomResponse</c> monta a resposta HTTP.
/// </summary>
public interface INotificadorService
{
    void NotificarErro(string mensagem, string? campo = null);
    void NotificarAlerta(string mensagem, string? campo = null);
    void NotificarSucesso(string mensagem);

    /// <summary>Indica que o recurso solicitado não existe (gera HTTP 404).</summary>
    void NotificarNaoEncontrado(string mensagem);

    bool TemErros { get; }
    bool RecursoNaoEncontrado { get; }
    IReadOnlyCollection<Notificacao> Notificacoes { get; }
    void Limpar();
}

public sealed class NotificadorService : INotificadorService
{
    private readonly List<Notificacao> _notificacoes = [];

    public bool RecursoNaoEncontrado { get; private set; }

    public void NotificarErro(string mensagem, string? campo = null) =>
        _notificacoes.Add(new Notificacao(TipoNotificacao.Erro, mensagem, campo));

    public void NotificarAlerta(string mensagem, string? campo = null) =>
        _notificacoes.Add(new Notificacao(TipoNotificacao.Alerta, mensagem, campo));

    public void NotificarSucesso(string mensagem) =>
        _notificacoes.Add(new Notificacao(TipoNotificacao.Sucesso, mensagem));

    public void NotificarNaoEncontrado(string mensagem)
    {
        RecursoNaoEncontrado = true;
        NotificarErro(mensagem);
    }

    public bool TemErros => _notificacoes.Any(n => n.Tipo == TipoNotificacao.Erro);

    public IReadOnlyCollection<Notificacao> Notificacoes => _notificacoes.AsReadOnly();

    public void Limpar()
    {
        _notificacoes.Clear();
        RecursoNaoEncontrado = false;
    }
}
