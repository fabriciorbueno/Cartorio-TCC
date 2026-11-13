namespace CartorioTcc.Business.Core.ValueObjects;

/// <summary>
/// Objeto de valor de endereço (o modelo original repetia estes campos em Clientes, Conjuges,
/// Funcionarios e Imoveis - aqui ele é um "owned type" mapeado nas mesmas tabelas).
/// </summary>
public sealed record Endereco
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
