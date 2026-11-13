using System.Globalization;
using System.Text;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Enums;

namespace CartorioTcc.Business.Builders;

/// <summary>
/// Calculadora oficial de Emolumentos e Custas Notariais conforme a Tabela I dos Tabelionatos de Notas
/// do Estado de São Paulo (Colégio Notarial do Brasil - CNB/SP, Lei Estadual nº 11.331/2002) e
/// previsão da Tabela II de Registro de Imóveis (mencionadas nas Atas de Reunião e no UC-06 do TCC).
/// </summary>
public static class TabelaCustasNotariaisCalculator
{
    public const decimal ValorUfespReferencia = 35.36m;

    private static readonly FaixaCustasDto[] FaixasPadrao =
    [
        CriarFaixa(1, "Faixa 1 — Até R$ 1.500,00 (sem valor declarado / mínimo)", 0.01m, 1_500.00m, 198.40m, 56.38m, 41.40m, 10.44m, 13.62m, 9.52m, 165.00m),
        CriarFaixa(2, "Faixa 2 — De R$ 1.500,01 até R$ 5.800,00", 1_500.01m, 5_800.00m, 312.60m, 88.84m, 65.24m, 16.45m, 21.46m, 15.01m, 240.00m),
        CriarFaixa(3, "Faixa 3 — De R$ 5.800,01 até R$ 9.650,00", 5_800.01m, 9_650.00m, 495.80m, 140.90m, 103.48m, 26.09m, 34.04m, 23.80m, 370.00m),
        CriarFaixa(4, "Faixa 4 — De R$ 9.650,01 até R$ 19.300,00", 9_650.01m, 19_300.00m, 698.20m, 198.43m, 145.72m, 36.75m, 47.94m, 33.51m, 515.00m),
        CriarFaixa(5, "Faixa 5 — De R$ 19.300,01 até R$ 38.600,00", 19_300.01m, 38_600.00m, 965.40m, 274.37m, 201.49m, 50.81m, 66.29m, 46.34m, 710.00m),
        CriarFaixa(6, "Faixa 6 — De R$ 38.600,01 até R$ 77.200,00", 38_600.01m, 77_200.00m, 1_195.00m, 339.62m, 249.41m, 62.90m, 82.06m, 57.36m, 890.00m),
        CriarFaixa(7, "Faixa 7 — De R$ 77.200,01 até R$ 115.800,00", 77_200.01m, 115_800.00m, 1_445.00m, 410.67m, 301.58m, 76.05m, 99.22m, 69.36m, 1_120.00m),
        CriarFaixa(8, "Faixa 8 — De R$ 115.800,01 até R$ 154.400,00", 115_800.01m, 154_400.00m, 1_720.00m, 488.82m, 358.98m, 90.52m, 118.10m, 82.56m, 1_360.00m),
        CriarFaixa(9, "Faixa 9 — De R$ 154.400,01 até R$ 193.000,00", 154_400.01m, 193_000.00m, 1_960.00m, 557.03m, 409.07m, 103.16m, 134.58m, 94.08m, 1_560.00m),
        CriarFaixa(10, "Faixa 10 — De R$ 193.000,01 até R$ 308.800,00", 193_000.01m, 308_800.00m, 2_255.00m, 640.87m, 470.64m, 118.68m, 154.84m, 108.24m, 1_820.00m),
        CriarFaixa(11, "Faixa 11 — De R$ 308.800,01 até R$ 617.600,00", 308_800.01m, 617_600.00m, 2_690.00m, 764.50m, 561.43m, 141.58m, 184.71m, 129.12m, 2_210.00m),
        CriarFaixa(12, "Faixa 12 — De R$ 617.600,01 até R$ 926.400,00", 617_600.01m, 926_400.00m, 3_085.00m, 876.75m, 643.87m, 162.37m, 211.83m, 148.08m, 2_590.00m),
        CriarFaixa(13, "Faixa 13 — De R$ 926.400,01 até R$ 1.250.000,00", 926_400.01m, 1_250_000.00m, 3_495.00m, 993.28m, 729.44m, 183.95m, 239.98m, 167.76m, 2_980.00m),
        CriarFaixa(14, "Faixa 14 — Acima de R$ 1.250.000,00", 1_250_000.01m, 999_999_999.99m, 4_120.00m, 1_170.90m, 859.88m, 216.84m, 282.90m, 197.76m, 3_520.00m)
    ];

    public static IReadOnlyList<FaixaCustasDto> ObterTabelaFaixas() => FaixasPadrao;

    public static Orcamento Calcular(CalcularOrcamentoRequest request)
    {
        var faixa = FaixasPadrao.FirstOrDefault(f =>
                        request.ValorBaseAto >= f.ValorMinimo && request.ValorBaseAto <= f.ValorMaximo)
                    ?? FaixasPadrao[^1];

        // Descontos e adicionais previstos nos campos de Orçamentos do TCC:
        // - Ato não oneroso: redução de 20% sobre a base notarial
        // - Hipoteca / Alienação fiduciária simultânea: acréscimo de 25% pela segunda garantia no mesmo instrumento
        // - Até 30 Salários Mínimos (1ª aquisição / SFH - Lei 6.015 art. 290): redução de 50% nos emolumentos
        // - Até 2.500 UFESPs (habitação de interesse social / PMCMV): redução de 40%
        var fator = 1.00m;
        if (!request.Oneroso) fator *= 0.80m;
        if (request.Hipoteca) fator *= 1.25m;
        if (request.Salarios30) fator *= 0.50m;
        else if (request.Ufesp2500) fator *= 0.60m;

        var emolumentosTabelionato = Arredondar(faixa.EmolumentosTabeliao * fator);
        var emolumentosEstado = Arredondar(faixa.ReceitaEstado * fator);
        var contribuicaoIpesp = Arredondar(faixa.ContribuicaoIpesp * fator);
        var fundoRegistroCivil = Arredondar(faixa.FundoRegistroCivil * fator);
        var taxaTribunalJustica = Arredondar(faixa.TaxaTribunalJustica * fator);
        var fundoMinisterioPublico = Arredondar(faixa.FundoMinisterioPublico * fator);
        var issqnEstimado = Arredondar(emolumentosTabelionato * 0.05m);
        var valorCertidao = request.CertidaoInclusa ? 74.92m : 0.00m;
        var previsaoRegistroImoveis = Arredondar(faixa.EstimativaRegistroImoveis * (request.Salarios30 ? 0.50m : 1.00m));
        var outrosValores = Math.Max(0m, Arredondar(request.OutrosValores));

        var total = emolumentosTabelionato
                    + emolumentosEstado
                    + contribuicaoIpesp
                    + fundoRegistroCivil
                    + taxaTribunalJustica
                    + fundoMinisterioPublico
                    + issqnEstimado
                    + valorCertidao
                    + previsaoRegistroImoveis
                    + outrosValores;

        return new Orcamento(
            request.IdCliente,
            Arredondar(request.ValorBaseAto),
            request.Oneroso,
            request.Hipoteca,
            request.Ufesp2500,
            request.Salarios30,
            request.CertidaoInclusa,
            outrosValores,
            faixa.DescricaoFaixa,
            emolumentosTabelionato,
            emolumentosEstado,
            contribuicaoIpesp,
            fundoRegistroCivil,
            taxaTribunalJustica,
            fundoMinisterioPublico,
            issqnEstimado,
            valorCertidao,
            previsaoRegistroImoveis,
            Arredondar(total));
    }

    private static FaixaCustasDto CriarFaixa(
        int ordem,
        string descricao,
        decimal min,
        decimal max,
        decimal tabeliao,
        decimal estado,
        decimal ipesp,
        decimal registroCivil,
        decimal tj,
        decimal mp,
        decimal registroImoveis)
    {
        var totalNotarial = tabeliao + estado + ipesp + registroCivil + tj + mp;
        return new FaixaCustasDto(
            ordem,
            descricao,
            min,
            max,
            tabeliao,
            estado,
            ipesp,
            registroCivil,
            tj,
            mp,
            totalNotarial,
            registroImoveis);
    }

    private static decimal Arredondar(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Builder da minuta (redação jurídica) da Escritura Pública para conferência prévia em tela (UC-09) e emissão de Certidão (UC-11).
/// </summary>
public static class EscrituraMinutaBuilder
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static string GerarTextoMinuta(
        Cartorio cartorio,
        Ato ato,
        Protocolo protocolo,
        Funcionario escrevente,
        Cliente outorgante,
        Cliente outorgado,
        Imovel? imovel,
        CertidaoOutroOrgao? certidaoOutroOrgao,
        int numeroLivro,
        int folhaInicial,
        int folhaFinal,
        decimal valorOperacao,
        DateTime dataReferencia)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ESCRITURA PÚBLICA DE {ato.TipoAto.ToUpperInvariant()}");
        sb.AppendLine($"{cartorio.NomeCartorio} — CNPJ {cartorio.CnpjCartorio}");
        sb.AppendLine($"Livro nº {numeroLivro:D4} | Folhas {folhaInicial:D3} a {folhaFinal:D3} | Protocolo nº {protocolo.NumeroProtocolo}");
        sb.AppendLine(new string('-', 88));
        sb.AppendLine();
        sb.AppendLine(
            $"SAIBAM quantos este público instrumento de escritura de {ato.TipoAto} virem que, aos " +
            $"{dataReferencia.ToString("dd 'de' MMMM 'de' yyyy", PtBr)}, nesta cidade e Comarca, perante mim, " +
            $"{escrevente.NomeFuncionario}, Escrevente Autorizado deste {cartorio.NomeCartorio}, compareceram as partes entre si " +
            "justas e contratadas, a saber:");
        sb.AppendLine();
        sb.AppendLine($"I — OUTORGANTE(S) / TRANSMITENTE(S): {QualificarParte(outorgante)}");
        sb.AppendLine();
        sb.AppendLine($"II — OUTORGADO(S) / ADQUIRENTE(S): {QualificarParte(outorgado)}");
        sb.AppendLine();

        if (imovel is not null)
        {
            sb.AppendLine("III — DO OBJETO / IMÓVEL:");
            sb.AppendLine(
                $"Imóvel matriculado sob o nº {imovel.NumeroRegistro} perante o {imovel.CartorioRegistroImoveis ?? "Cartório de Registro de Imóveis competente"}, " +
                $"cadastrado na {imovel.NomePrefeitura}" +
                $"{(string.IsNullOrWhiteSpace(imovel.InscricaoMunicipal) ? string.Empty : $" sob inscrição municipal nº {imovel.InscricaoMunicipal}")}, " +
                $"situado em {imovel.Endereco.Formatado}. " +
                $"{(string.IsNullOrWhiteSpace(imovel.DescricaoImovel) ? string.Empty : imovel.DescricaoImovel)}");
            sb.AppendLine(
                $"Valor Venal de Referência: {imovel.ValorVenal.ToString("C", PtBr)}" +
                $"{(imovel.AreaMetrosQuadrados is > 0 ? $" | Área: {imovel.AreaMetrosQuadrados:N2} m² (R$ {imovel.ValorPorMetroQuadrado:N2}/m²)" : string.Empty)}.");
            sb.AppendLine();
        }

        sb.AppendLine("IV — DO PREÇO, FORMA DE PAGAMENTO E DECLARAÇÕES:");
        sb.AppendLine(
            $"Pelo presente ato notarial de {ato.TipoAto}, o(a) Outorgante transmite ao(à) Outorgado(a) todos os direitos, " +
            $"domínio, posse e ação pelo valor certo e ajustado de {valorOperacao.ToString("C", PtBr)}, declarado quitado neste ato.");

        if (certidaoOutroOrgao is not null)
        {
            sb.AppendLine(
                $"Foram apresentadas e arquivadas as certidões exigidas por lei, incluindo certidão expedida por " +
                $"{certidaoOutroOrgao.CartorioCertidaoOutroOrgao} (Livro {certidaoOutroOrgao.LivroCertidaoOutroOrgao}, " +
                $"Folhas {certidaoOutroOrgao.FolhasCertidaoOutroOrgao}, Termo {certidaoOutroOrgao.TermoCertidaoOutroOrgao}, " +
                $"em {certidaoOutroOrgao.DataCertidaoOutroOrgao:dd/MM/yyyy}).");
        }

        sb.AppendLine();
        sb.AppendLine(
            "V — DO ENCERRAMENTO: Assim o disseram, dou fé, pediram-me e lhes lavrei este instrumento que, feito e lido em voz alta, " +
            "acharam em tudo conforme, aceitaram, outorgaram e assinam. Emitida a Declaração sobre Operações Imobiliárias (DOI) à " +
            "Receita Federal do Brasil no prazo legal de até 7 (sete) dias subsequentes à lavratura.");

        return sb.ToString();
    }

    private static string QualificarParte(Cliente c)
    {
        var sb = new StringBuilder();
        sb.Append(
            $"{c.NomeCliente}, nacionalidade {c.NacionalidadeCliente}, estado civil {c.EstadoCivilCliente}, " +
            $"profissão {c.ProfissaoCliente}, portador(a) da cédula de identidade RG nº {c.RgCliente} " +
            $"({c.OrgaoExpedidorCliente}/{c.EstadoOrgaoExpedidorCliente}) e inscrito(a) no CPF sob o nº {c.CpfCliente}, " +
            $"nascido(a) em {c.DataNascCliente:dd/MM/yyyy}, e-mail {c.EmailCliente}, telefone {c.TelefoneCliente}, " +
            $"residente e domiciliado(a) em {c.Endereco.Formatado}");

        if (c.Conjuge is not null)
        {
            sb.Append(
                $", casado(a) com {c.Conjuge.NomeConjuge}, CPF nº {c.Conjuge.CpfConjuge}, RG nº {c.Conjuge.RgConjuge} " +
                $"({c.Conjuge.OrgaoExpedidorConjuge}/{c.Conjuge.EstadoOrgaoExpedidorConjuge}), profissão {c.Conjuge.ProfissaoConjuge}");
        }

        if (c.CertidaoCasamento is not null)
        {
            sb.Append(
                $" (Certidão de Casamento lavrada no {c.CertidaoCasamento.CartorioCertidaoCasamento}, " +
                $"Livro {c.CertidaoCasamento.LivroCertidaoCasamento}, Folhas {c.CertidaoCasamento.FolhasCertidaoCasamento}, " +
                $"Termo {c.CertidaoCasamento.TermoCertidaoCasamento}, em {c.CertidaoCasamento.DataCertidaoCasamento:dd/MM/yyyy})");
        }

        sb.Append('.');
        return sb.ToString();
    }

    public static string DescreverStatusEscritura(StatusEscritura status) => status switch
    {
        StatusEscritura.Inativa => "Inativa / Cancelada",
        StatusEscritura.AguardandoDocumentacao => "Aguardando documentação",
        StatusEscritura.EmProcessoDeEscrituracao => "Em processo de escrituração",
        StatusEscritura.EmProcessoDeImpressao => "Em processo de impressão",
        StatusEscritura.Finalizada => "Finalizado – aguardando retirada",
        _ => status.ToString()
    };
}
