namespace CartorioTcc.Business.Core.Models;

/// <summary>Filtro base de paginação (o TCC define 20 registros por página como padrão).</summary>
public class FiltroPaginado
{
    public const int TamanhoPadrao = 20;
    private int _pagina = 1;
    private int _tamanho = TamanhoPadrao;

    public int Pagina
    {
        get => _pagina;
        set => _pagina = value < 1 ? 1 : value;
    }

    public int Tamanho
    {
        get => _tamanho;
        set => _tamanho = value is < 1 or > 200 ? TamanhoPadrao : value;
    }

    public string? Termo { get; set; }
    public string? Ordenacao { get; set; }

    public int Skip => (Pagina - 1) * Tamanho;
}

public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Itens, int Total, int Pagina, int Tamanho)
{
    public int TotalPaginas => Tamanho == 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamanho);
}

public sealed record OpcaoDto(int Id, string Descricao, string? Complemento = null);

public sealed record ArquivoGerado(string NomeArquivo, string ContentType, byte[] Conteudo);
