using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Enums;

namespace CartorioTcc.IntegrationTests;

public sealed class CartorioTccApiIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ECatalogos_DevemRetornarStatus200OkComDadosIniciaisDoTcc()
    {
        var healthResp = await _client.GetAsync("/api/catalogos/health");
        healthResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var perfisResp = await _client.GetAsync("/api/catalogos/perfis");
        perfisResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await perfisResp.Content.ReadAsStringAsync();
        json.Should().Contain("Administrador").And.Contain("Tabelião").And.Contain("Escrevente");
    }

    [Fact]
    public async Task UC06_SimularOrcamento_DeveRetornarCalculoTabelaCustasCnbSp()
    {
        var req = new CalcularOrcamentoRequest(500_000m, 1, true, false, true, false, true, 0m);
        var resp = await _client.PostAsJsonAsync("/api/orcamentos/simular", req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadAsStringAsync();
        content.Should().Contain("617.600,00");
    }

    [Fact]
    public async Task UC12_ConsultarAndamentoPublico_Protocolo4119914266_DeveRetornarEtapasDoProcesso()
    {
        var resp = await _client.GetAsync("/api/protocolos/publico/4119914266");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("4119914266")
            .And.Contain("Fabricio Lima dos Santos");
    }

    [Fact]
    public async Task UC10_ExportarDoiXml_DeveGerarArquivoXmlNoPadraoDaReceitaFederal()
    {
        var resp = await _client.GetAsync("/api/escrituras/1/exportar-doi-xml");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType?.MediaType.Should().Be("application/xml");

        var xml = await resp.Content.ReadAsStringAsync();
        xml.Should().Contain("DeclaracaoOperacoesImobiliarias")
           .And.Contain("4119914266")
           .And.Contain("Avenida Prestes Maia");
    }

    [Fact]
    public async Task UC11_EmitirCertidaoPorProtocolo_DeveGerarPdfValido()
    {
        var resp = await _client.GetAsync("/api/escrituras/certidao-por-protocolo/4119914266");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(500);
    }

    [Fact]
    public async Task UC04_DashboardERelatorios_DevemRetornarIndicadoresERelatorioEmTela()
    {
        var dashResp = await _client.GetAsync("/api/relatorios/dashboard");
        dashResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var relResp = await _client.PostAsJsonAsync("/api/relatorios/gerar",
            new FiltroRelatorioRequest(TipoRelatorioEnum.EscriturasCriadas));
        relResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var relJson = await relResp.Content.ReadAsStringAsync();
        relJson.Should().Contain("Escrituras");
    }

    [Fact]
    public async Task UC05_ExecutarBackupManualCriptografado_DeveGerarHashSha256()
    {
        var resp = await _client.PostAsJsonAsync("/api/backups/manual",
            new RealizarBackupManualCommand("Backup manual de teste integrado", 1));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain(".saec.enc");
    }
}
