using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Interfaces;

namespace CartorioTcc.Infra.Documents;

public sealed class PdfGeneratorService : IPdfGeneratorService
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    static PdfGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ArquivoGerado GerarCertidaoEscrituraPdf(EscrituraDetalheDto escritura, DateTime dataEmissao, string emitidoPor)
    {
        var nomeArquivo = $"Certidao_Livro_{escritura.NumeroLivro}_Folha_{escritura.FolhaInicial}_{escritura.NumeroProtocolo}.pdf";

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10.5f));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("REPÚBLICA FEDERATIVA DO BRASIL - ESTADO DE SÃO PAULO").Bold().FontSize(11);
                    col.Item().AlignCenter().Text(escritura.NomeCartorio.ToUpperInvariant()).Bold().FontSize(13).FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(4).AlignCenter().Text($"CERTIDÃO DE ESCRITURA PÚBLICA — LIVRO Nº {escritura.NumeroLivro} / FOLHAS {escritura.FolhaInicial} A {escritura.FolhaFinal}").SemiBold().FontSize(11);
                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(info =>
                    {
                        info.Spacing(3);
                        info.Item().Text($"Protocolo SAEC: {escritura.NumeroProtocolo} | Escritura Nº: {escritura.NumeroEscritura}").SemiBold();
                        info.Item().Text($"Ato Notarial: {escritura.TipoAto}");
                        info.Item().Text($"Outorgante(s): {escritura.Outorgante.NomeCliente} (CPF: {escritura.Outorgante.CpfCliente})");
                        info.Item().Text($"Outorgado(s): {escritura.Outorgado.NomeCliente} (CPF: {escritura.Outorgado.CpfCliente})");
                        if (escritura.Imovel is not null)
                        {
                            info.Item().Text($"Imóvel Objeto: Matrícula nº {escritura.Imovel.NumeroRegistro} ({escritura.Imovel.CartorioRegistroImoveis})");
                            info.Item().Text($"Valor da Operação: {escritura.ValorOperacao.ToString("C2", PtBr)} | Valor Venal: {escritura.Imovel.ValorVenal.ToString("C2", PtBr)}");
                        }
                    });

                    col.Item().PaddingTop(6).Text("TEOR INTEGRAL DA ESCRITURA LAVRADA:").Bold().FontSize(11);
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Text(escritura.TextoMinuta).Justify();

                    col.Item().PaddingTop(25).AlignCenter().Column(sign =>
                    {
                        sign.Item().AlignCenter().Text("_____________________________________________________________");
                        sign.Item().AlignCenter().Text($"{escritura.NomeEscrevente} (Emitido por: {emitidoPor})").Bold();
                        sign.Item().AlignCenter().Text(escritura.NomeCartorio);
                        sign.Item().PaddingTop(4).AlignCenter().Text($"Emitida em Campinas/SP, {dataEmissao.ToString("dd/MM/yyyy 'às' HH:mm", PtBr)}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("SAEC — Sistema Automatizador de Escrituração para Cartórios | Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();

        return new ArquivoGerado(nomeArquivo, "application/pdf", bytes);
    }

    public ArquivoGerado GerarProtocoloAtendimentoPdf(ProtocoloDto protocolo)
    {
        var nomeArquivo = $"Protocolo_{protocolo.NumeroProtocolo}.pdf";
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("2º TABELIONATO DE NOTAS DE CAMPINAS — SAEC").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                    col.Item().AlignCenter().Text("COMPROVANTE DE PROTOCOLO DE ATENDIMENTO E RECEBIMENTO DE DOCUMENTOS").SemiBold().FontSize(11);
                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten2).Padding(14).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("NÚMERO DO PROTOCOLO PARA CONSULTA ONLINE / MOBILE (UC-12):").FontSize(9.5f);
                            c.Item().Text(protocolo.NumeroProtocolo).Bold().FontSize(20).FontColor(Colors.Blue.Darken4);
                        });
                        row.ConstantItem(180).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Data Recebimento: {protocolo.DtCriacao.ToString("dd/MM/yyyy", PtBr)}").SemiBold();
                            c.Item().Text($"Status: {protocolo.StatusAndamentoDescricao}");
                        });
                    });

                    col.Item().Text($"Solicitante / Apresentante: {protocolo.NomeCliente}").SemiBold();
                    col.Item().Text($"CPF: {protocolo.CpfCliente} | Telefone: {protocolo.TelefoneCliente}");
                    col.Item().Text($"E-mail: {protocolo.EmailCliente}");
                    col.Item().Text($"Serviço / Ato Solicitado: {protocolo.TipoAto ?? protocolo.NaturezaPedido ?? "Escritura Pública"}");
                    col.Item().Text($"Escrevente Responsável: {protocolo.NomeEscrevente ?? "Equipe Notarial"}");
                    col.Item().Text($"Documentos Apresentados: {protocolo.DocumentosApresentados ?? "RG, CPF, Certidão de Casamento e Matrícula atualizada"}");
                    if (!string.IsNullOrWhiteSpace(protocolo.Observacoes))
                        col.Item().Text($"Observações: {protocolo.Observacoes}");

                    col.Item().PaddingTop(16).Background(Colors.Grey.Lighten4).Padding(10)
                        .Text("Acompanhe o andamento da sua escritura em tempo real na área pública de Consulta de Andamento informando o número de protocolo acima.");

                    col.Item().PaddingTop(40).AlignCenter().Text("______________________________________________________");
                    col.Item().AlignCenter().Text("Assinatura do Responsável pelo Recebimento - SAEC");
                });
            });
        }).GeneratePdf();

        return new ArquivoGerado(nomeArquivo, "application/pdf", bytes);
    }

    public ArquivoGerado GerarProtocoloRetiradaPdf(EscrituraDetalheDto escritura)
    {
        var nomeArquivo = $"Protocolo_Retirada_{escritura.NumeroProtocolo}.pdf";

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("2º TABELIONATO DE NOTAS DE CAMPINAS — SAEC").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                    col.Item().AlignCenter().Text("TERMO E PROTOCOLO DE RETIRADA DE ESCRITURA E DOCUMENTOS").SemiBold().FontSize(11);
                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Protocolo nº: {escritura.NumeroProtocolo}").Bold().FontSize(13);
                    col.Item().Text($"Escritura Nº {escritura.NumeroEscritura}: Livro nº {escritura.NumeroLivro} / Folhas {escritura.FolhaInicial} a {escritura.FolhaFinal} — {escritura.TipoAto}");
                    col.Item().Text($"Outorgante: {escritura.Outorgante.NomeCliente} (CPF: {escritura.Outorgante.CpfCliente})");
                    col.Item().Text($"Outorgado: {escritura.Outorgado.NomeCliente} (CPF: {escritura.Outorgado.CpfCliente})");
                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().Text($"Data/Hora da Retirada: {(escritura.DataRetiradaDocumentos ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm", PtBr)}");
                    col.Item().Text($"Observações de Retirada: {escritura.ObservacoesRetirada ?? "Documentação entregue ao apresentante."}");
                    col.Item().PaddingTop(14).Text(
                        "Declaro ter recebido do 2º Tabelionato de Notas de Campinas o traslado/certidão da escritura pública acima identificada, bem como os documentos originais apresentados para sua lavratura, dando plena quitação de entrega.");

                    col.Item().PaddingTop(45).AlignCenter().Text("_____________________________________________________________");
                    col.Item().AlignCenter().Text($"{escritura.Outorgado.NomeCliente} (CPF: {escritura.Outorgado.CpfCliente})").Bold();
                });
            });
        }).GeneratePdf();

        return new ArquivoGerado(nomeArquivo, "application/pdf", bytes);
    }

    public ArquivoGerado GerarOrcamentoPdf(OrcamentoDto orcamento)
    {
        var nomeArquivo = $"Orcamento_SAEC_{orcamento.Id:D5}.pdf";
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("2º TABELIONATO DE NOTAS DE CAMPINAS — SAEC").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                    col.Item().AlignCenter().Text("ORÇAMENTO DE EMOLUMENTOS E CUSTAS NOTARIAIS (TABELA CNB/SP)").SemiBold().FontSize(11);
                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Orçamento Nº: {orcamento.Id:D5} | Data: {orcamento.DtCriacao.ToString("dd/MM/yyyy HH:mm", PtBr)}").Bold();
                    col.Item().Text($"Cliente Solicitante: {orcamento.NomeCliente ?? "Atendimento Balcão"}");
                    col.Item().Text($"Faixa CNB/SP Enquadrada: {orcamento.FaixaDescricao}");
                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(12).Column(calc =>
                    {
                        calc.Spacing(5);
                        calc.Item().Text($"Valor Base do Ato / Imóvel: {orcamento.ValorBaseAto.ToString("C2", PtBr)}").Bold();
                        calc.Item().Text($"Emolumentos Tabelionato: {orcamento.EmolumentosTabelionato.ToString("C2", PtBr)}");
                        calc.Item().Text($"Receita do Estado: {orcamento.EmolumentosEstado.ToString("C2", PtBr)}");
                        calc.Item().Text($"Contribuição IPESP / Carteira das Serventias: {orcamento.ContribuicaoIpesp.ToString("C2", PtBr)}");
                        calc.Item().Text($"Fundo Registro Civil: {orcamento.FundoRegistroCivil.ToString("C2", PtBr)} | Taxa TJ/SP: {orcamento.TaxaTribunalJustica.ToString("C2", PtBr)}");
                        calc.Item().Text($"Fundo Ministério Público: {orcamento.FundoMinisterioPublico.ToString("C2", PtBr)} | ISSQN Estimado: {orcamento.IssqnEstimado.ToString("C2", PtBr)}");
                        calc.Item().Text($"Certidão / Traslado: {orcamento.ValorCertidao.ToString("C2", PtBr)}");
                        calc.Item().Text($"Previsão Registro de Imóveis (Tabela II): {orcamento.PrevisaoRegistroImoveis.ToString("C2", PtBr)}");
                        calc.Item().Text($"Outros Valores: {orcamento.OutrosValores.ToString("C2", PtBr)}");
                        calc.Item().PaddingTop(4).Text($"VALOR TOTAL ESTIMADO DO ORÇAMENTO: {orcamento.ValorTotalOrcamento.ToString("C2", PtBr)}")
                            .Bold().FontSize(13).FontColor(Colors.Blue.Darken3);
                    });
                });
            });
        }).GeneratePdf();

        return new ArquivoGerado(nomeArquivo, "application/pdf", bytes);
    }

    public ArquivoGerado GerarRelatorioPdf(RelatorioResultadoDto relatorio)
    {
        var nomeArquivo = $"Relatorio_{relatorio.TipoRelatorio}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("2º TABELIONATO DE NOTAS DE CAMPINAS — SAEC").Bold().FontSize(13).FontColor(Colors.Blue.Darken3);
                    col.Item().AlignCenter().Text(relatorio.TituloRelatorio.ToUpperInvariant()).SemiBold().FontSize(11);
                    col.Item().AlignCenter().Text($"Gerado em {relatorio.DataGeracao.ToString("dd/MM/yyyy HH:mm", PtBr)} por {relatorio.GeradoPor} | Total de Registros: {relatorio.TotalRegistros} | Soma: {relatorio.SomaValores.ToString("C2", PtBr)}").FontSize(9);
                    col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(85);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.3f);
                        columns.ConstantColumn(75);
                        columns.ConstantColumn(85);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Código").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Título / Parte Principal").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Parte Secundária").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Data").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Status / Valor").FontColor(Colors.White).Bold();
                    });

                    foreach (var item in relatorio.Itens)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Codigo);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(c =>
                        {
                            c.Item().Text(item.Titulo).SemiBold();
                            c.Item().Text(item.PartePrincipal).FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.ParteSecundaria);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.DataReferencia.ToString("dd/MM/yyyy", PtBr));
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(
                            item.Valor.HasValue ? $"{item.Status}\n{item.Valor.Value.ToString("C2", PtBr)}" : item.Status);
                    }
                });
            });
        }).GeneratePdf();

        return new ArquivoGerado(nomeArquivo, "application/pdf", bytes);
    }
}
