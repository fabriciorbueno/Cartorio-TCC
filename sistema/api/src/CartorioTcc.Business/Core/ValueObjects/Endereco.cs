namespace CartorioTcc.Business.Core.ValueObjects;

/// <summary>
/// Objeto de valor de endereço (o modelo original repetia estes campos em Clientes, Conjuges,
/// Funcionarios e Imoveis - aqui ele é um "owned type" mapeado nas mesmas tabelas).
/// Declarado como class (com semântica de clonagem explícita) para que o ChangeTracker do EF Core
/// rastreie cada instância de Owned Entity por identidade de referência sem colisão entre Cliente e Cônjuge.
/// </summary>
public sealed class Endereco
{
    public string Cep { get; init; } = string.Empty;
    public string Pais { get; init; } = "Brasil";
    public string Estado { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Bairro { get; init; } = string.Empty;
    public string Logradouro { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string? Complemento { get; init; }

    public static Endereco Vazio => new();

    public Endereco Clonar() => new()
    {
        Cep = Cep,
        Pais = Pais,
        Estado = Estado,
        Cidade = Cidade,
        Bairro = Bairro,
        Logradouro = Logradouro,
        Numero = Numero,
        Complemento = Complemento
    };

    public string Formatado =>
        string.Join(", ", new[]
        {
            $"{Logradouro} {Numero}".Trim(),
            Complemento,
            Bairro,
            string.IsNullOrWhiteSpace(Cidade) ? null : $"{Cidade}/{Estado}",
            string.IsNullOrWhiteSpace(Cep) ? null : $"CEP {Cep}"
        }.Where(p => !string.IsNullOrWhiteSpace(p)));
}
