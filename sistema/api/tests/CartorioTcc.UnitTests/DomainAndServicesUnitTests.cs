using FluentAssertions;
using NSubstitute;
using Xunit;
using CartorioTcc.Business.Builders;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Core.Notifications;
using CartorioTcc.Business.Core.Validations;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Business.Services;

namespace CartorioTcc.UnitTests;

public sealed class DomainAndServicesUnitTests
{
    [Theory]
    [InlineData("52998224725", true)]
    [InlineData("11144477735", true)]
    [InlineData("12345678909", true)]
    [InlineData("11111111111", false)]
    [InlineData("12345678900", false)]
    [InlineData("123", false)]
    public void Validador_DeveValidarCpfCorretamente(string cpf, bool esperado)
    {
        Validador.CpfValido(cpf).Should().Be(esperado);
    }

    [Theory]
    [InlineData("Vanderlei Minussi", true)]
    [InlineData("Maria José da Conceição", true)]
    [InlineData("Vanderlei 123", false)]
    [InlineData("", false)]
    public void Validador_DeveValidarNomeApenasLetrasConformeTcc(string nome, bool esperado)
    {
        Validador.NomeValido(nome).Should().Be(esperado);
    }

    [Theory]
    [InlineData("Asd@123", true)]
    [InlineData("123456", true)]
    [InlineData("12345", false)]
    [InlineData("123456789012", false)]
    public void Validador_DeveValidarTamanhoDeSenhaEntre6E11Caracteres(string senha, bool esperado)
    {
        Validador.SenhaValida(senha).Should().Be(esperado);
    }

    [Fact]
    public void TabelaCustasNotariaisCalculator_DeveCalcularOrcamento500MilConformeCasoDeTesteTcc()
    {
        // Caso de Teste CT-06 do TCC: Valor R$ 500.000,00, Oneroso, UFESP2500, Certidão
        var resultado = TabelaCustasNotariaisCalculator.Calcular(
            new CalcularOrcamentoRequest(
                ValorBaseAto: 500_000m,
                IdCliente: 1,
                Oneroso: true,
                Hipoteca: false,
                Ufesp2500: true,
                Salarios30: false,
                CertidaoInclusa: true,
                OutrosValores: 0m));

        resultado.EmolumentosTabelionato.Should().BeGreaterThan(0m);
        resultado.ValorCertidao.Should().Be(74.92m);
        resultado.ValorTotalOrcamento.Should().BeGreaterThan(resultado.EmolumentosTabelionato);
        resultado.FaixaDescricao.Should().Contain("617.600,00");
    }

    [Fact]
    public async Task FuncionarioService_DeveNotificarErro_QuandoCpfOuEmailJaCadastrado()
    {
        // Arrange
        var notificador = new NotificadorService();
        var repo = Substitute.For<IFuncionarioRepository>();
        var query = Substitute.For<IFuncionarioQueryService>();
        var uow = Substitute.For<IUnitOfWork>();
        repo.UnitOfWork.Returns(uow);
        repo.ExisteCpfOuEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var sut = new FuncionarioService(notificador, repo, query);

        var command = new IncluirFuncionarioCommand(
            IdPerfil: 4,
            NomeFuncionario: "Vanderlei Minussi",
            DataNascFuncionario: new DateOnly(1986, 10, 12),
            DataAdmissao: new DateOnly(2015, 2, 1),
            EmailFuncionario: "vanderlei@saec.com",
            Senha: "123456",
            ConfirmarSenha: "123456",
            CpfFuncionario: "12345678909",
            RgFuncionario: "41122233X",
            TelefoneFuncionario: "1938871122",
            Cargo: "Auxiliar de Cartório",
            Endereco: new EnderecoDto("13183091", "Brasil", "SP", "Hortolândia", "Jd Nova Hortolândia", "Rua Antonio Carlos", "120"));

        // Act
        var dto = await sut.IncluirAsync(command);

        // Assert
        dto.Should().BeNull();
        notificador.TemErros.Should().BeTrue();
    }

    [Fact]
    public async Task ClienteService_DeveExigirConjugeECertidao_QuandoEstadoCivilCasado()
    {
        // Arrange (Caso de Teste CT-08 do TCC: Maria José Casada sem informar cônjuge)
        var notificador = new NotificadorService();
        var repo = Substitute.For<IClienteRepository>();
        var query = Substitute.For<IClienteQueryService>();
        var dtProvider = new DateTimeProvider();
        var sut = new ClienteService(notificador, repo, query, dtProvider);

        var commandSemConjuge = new SalvarClienteCommand(
            NomeCliente: "Maria José",
            RgCliente: "19887766",
            OrgaoExpedidorCliente: "SSP",
            NacionalidadeCliente: "Brasileiro(a)",
            EstadoOrgaoExpedidorCliente: "SP",
            CpfCliente: "15350946056",
            DataNascCliente: new DateOnly(1975, 1, 30),
            EmailCliente: "maria.jose@email.com",
            GeneroCliente: "F",
            EstadoCivilCliente: "Casado(a)",
            RegimeBens: "Comunhão Parcial de Bens",
            ProfissaoCliente: "Professora",
            TelefoneCliente: "1938874411",
            Endereco: new EnderecoDto("13183091", "Brasil", "SP", "Hortolândia", "Jd Nova Hortolândia", "Rua Antonio Carlos", "85"),
            Conjuge: null,
            CertidaoCasamento: null);

        // Act
        var dto = await sut.IncluirAsync(commandSemConjuge);

        // Assert
        dto.Should().BeNull();
        notificador.TemErros.Should().BeTrue();
    }

    [Fact]
    public async Task EscrituraService_DeveExigirMotivoObrigatorio_AoInativarEscritura()
    {
        // Arrange (RN: Inativação de escritura requer justificativa obrigatória para auditoria)
        var notificador = new NotificadorService();
        var escrituraRepo = Substitute.For<IEscrituraRepository>();
        var protocoloRepo = Substitute.For<IProtocoloRepository>();
        var clienteService = Substitute.For<IClienteService>();
        var queryService = Substitute.For<IEscrituraQueryService>();
        var doiGen = Substitute.For<IDoiExportGenerator>();
        var pdfGen = Substitute.For<IPdfGeneratorService>();
        var dtProvider = new DateTimeProvider();
        var userContext = Substitute.For<IUserContext>();

        var sut = new EscrituraService(
            notificador, escrituraRepo, protocoloRepo, clienteService,
            queryService, doiGen, pdfGen, dtProvider, userContext);

        // Act
        var ok = await sut.InativarAsync(new InativarEscrituraCommand(1, "   "));

        // Assert
        ok.Should().BeFalse();
        notificador.TemErros.Should().BeTrue();
    }
}
