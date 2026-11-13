using System.Globalization;
using System.Text;
using System.Xml.Linq;
using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Interfaces;

namespace CartorioTcc.Infra.Documents;

/// <summary>
/// UC-10: Gerar arquivo XML para Envio DOI (Declaração sobre Operações Imobiliárias - Receita Federal).
/// Segue o padrão do TCC: nome do arquivo "yyyy-MM-dd-numeroProtocolo.xml".
/// </summary>
public sealed class DoiExportGenerator : IDoiExportGenerator
{
    public ArquivoGerado GerarXmlDoi(EscrituraDetalheDto escritura, DateTime dataGeracao)
    {
        var dataArquivo = dataGeracao.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var nomeArquivo = $"{dataArquivo}-{escritura.NumeroProtocolo}.xml";

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement("DeclaracaoOperacoesImobiliarias",
                new XAttribute("versao", "3.1"),
                new XAttribute("orgaoDestino", "RFB-DOI"),
                new XElement("IdentificacaoServentia",
                    new XElement("Cartorio", escritura.NomeCartorio),
                    new XElement("Cidade", "Campinas"),
                    new XElement("UF", "SP")
                ),
                CriarElementoDeclaracao(escritura)
            )
        );

        return SerializarXml(doc, nomeArquivo);
    }

    public ArquivoGerado GerarXmlDoiLote(IReadOnlyList<EscrituraDetalheDto> escrituras, DateTime dataGeracao)
    {
        var dataArquivo = dataGeracao.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var nomeArquivo = $"{dataArquivo}-LOTE-DOI-{escrituras.Count}.xml";

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement("DeclaracaoOperacoesImobiliariasLote",
                new XAttribute("versao", "3.1"),
                new XAttribute("orgaoDestino", "RFB-DOI"),
                new XAttribute("quantidade", escrituras.Count),
                escrituras.Select(CriarElementoDeclaracao)
            )
        );

        return SerializarXml(doc, nomeArquivo);
    }

    private static XElement CriarElementoDeclaracao(EscrituraDetalheDto escritura)
    {
        return new XElement("Declaracao",
            new XElement("NumeroControle", escritura.NumeroProtocolo),
            new XElement("NumeroEscritura", escritura.NumeroEscritura),
            new XElement("Livro", escritura.NumeroLivro),
            new XElement("FolhaInicial", escritura.FolhaInicial),
            new XElement("FolhaFinal", escritura.FolhaFinal),
            new XElement("DataLavratura", (escritura.DataLavratura ?? escritura.DataInicioProcesso).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement("TipoAto", escritura.TipoAto),
            new XElement("AlienanteOutorgante",
                new XElement("Nome", escritura.Outorgante.NomeCliente),
                new XElement("NI", escritura.Outorgante.CpfCliente),
                new XElement("EstadoCivil", escritura.Outorgante.EstadoCivilCliente),
                new XElement("ConjugeNI", escritura.Outorgante.Conjuge?.CpfConjuge ?? string.Empty)
            ),
            new XElement("AdquirenteOutorgado",
                new XElement("Nome", escritura.Outorgado.NomeCliente),
                new XElement("NI", escritura.Outorgado.CpfCliente),
                new XElement("EstadoCivil", escritura.Outorgado.EstadoCivilCliente),
                new XElement("ConjugeNI", escritura.Outorgado.Conjuge?.CpfConjuge ?? string.Empty)
            ),
            new XElement("ImovelTransacionado",
                new XElement("Matricula", escritura.Imovel?.NumeroRegistro ?? string.Empty),
                new XElement("InscricaoMunicipal", escritura.Imovel?.InscricaoMunicipal ?? string.Empty),
                new XElement("Prefeitura", escritura.Imovel?.NomePrefeitura ?? "Prefeitura Municipal de Campinas"),
                new XElement("CartorioRegistroImoveis", escritura.Imovel?.CartorioRegistroImoveis ?? string.Empty),
                new XElement("Logradouro", escritura.Imovel?.Endereco.Logradouro ?? string.Empty),
                new XElement("Numero", escritura.Imovel?.Endereco.Numero ?? string.Empty),
                new XElement("Bairro", escritura.Imovel?.Endereco.Bairro ?? string.Empty),
                new XElement("Municipio", escritura.Imovel?.Endereco.Cidade ?? "Campinas"),
                new XElement("UF", escritura.Imovel?.Endereco.Estado ?? "SP"),
                new XElement("CEP", escritura.Imovel?.Endereco.Cep ?? string.Empty),
                new XElement("ValorOperacao", escritura.ValorOperacao.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement("ValorVenal", (escritura.Imovel?.ValorVenal ?? 0m).ToString("F2", CultureInfo.InvariantCulture))
            )
        );
    }

    private static ArquivoGerado SerializarXml(XDocument doc, string nomeArquivo)
    {
        using var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, new UTF8Encoding(false), leaveOpen: true))
        {
            doc.Save(writer);
        }

        return new ArquivoGerado(nomeArquivo, "application/xml", ms.ToArray());
    }
}
